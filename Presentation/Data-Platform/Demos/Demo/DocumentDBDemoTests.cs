using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data_Platform_Demos.DocumentDB;
using Data_Platform_Demos.SqlDatabase;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Product = Data_Platform_Demos.DocumentDB.Product;

namespace Data_Platform_Demos
{
	[TestClass]
	public class DocumentDBDemoTests
	{
		const string DatabaseId = "DemoDatabase";
		const string CollectionId = "DemoCollection";

		static DocumentDBRepository<Product> docDBRepo;

		[ClassInitialize]
		public static void ConnectToDocumentDB(TestContext context)
		{
			var documentDbConnectionData = GetDocumentDBConnectionData();
			var endpoint = documentDbConnectionData["endpoint"];
			var authKey = documentDbConnectionData["authkey"];

			docDBRepo = new DocumentDBRepository<Product>(
				endpoint,
				authKey,
				DatabaseId,
				CollectionId);
		}

		private static IDictionary<string, string> GetDocumentDBConnectionData()
		{
			return ConfigurationManager
				.ConnectionStrings["Azure DocumentDB Demo"]
				.ConnectionString
				.Split(';')
				.Select(s =>
				{
					var pair = s.Split('|');
					return new KeyValuePair<string, string>(pair.First(), pair.Last());
				})
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		[TestMethod]
		public void CreateDatabaseAndCollection()
		{
			var database = docDBRepo.Database().Result;
			var collection = docDBRepo.Collection().Result;

			Assert.IsNotNull(database);
			Assert.AreEqual(DatabaseId, database.Id);

			ConsoleWrite("----------------------------------------------");
			ConsoleWrite("Database:");
			ConsoleWrite("Id", database.Id);
			ConsoleWrite("SelfLink", database.SelfLink);
			ConsoleWrite("ResourceId", database.ResourceId);
			ConsoleWrite("CollectionsLink", database.CollectionsLink);
			ConsoleWrite("UsersLink", database.UsersLink);
			ConsoleWrite("----------------------------------------------");
			ConsoleWrite("Collection:");
			ConsoleWrite("Id", collection.Id);
			ConsoleWrite("SelfLink", collection.SelfLink);
			ConsoleWrite("ResourceId", collection.ResourceId);
			ConsoleWrite("DocumentsLink", collection.DocumentsLink);
			ConsoleWrite("ConflictsLink", collection.ConflictsLink);
			ConsoleWrite("StoredProceduresLink", collection.StoredProceduresLink);
			ConsoleWrite("TriggersLink", collection.TriggersLink);
			ConsoleWrite("UserDefinedFunctionsLink", collection.UserDefinedFunctionsLink);
			ConsoleWrite("IndexingPolicy", collection.IndexingPolicy);
			ConsoleWrite("----------------------------------------------");
		}

		[TestMethod]
		public void CreateData_Sequentially()
		{
			// 504 documents to be inserted
			var documentProducts = FetchProductsFromSqlDatabaseAsync().Result;

			var stopwatch = Stopwatch.StartNew();

			// Insert or replace all the documents in the database one by one...
			foreach (var product in documentProducts)
			{
				docDBRepo.UpdateDocumentAsync(product, p => p.id).Wait();
			}

			stopwatch.Stop();

			// You might want to stop this test after a while! It will complete but it takes a long time!
			Console.WriteLine("Updated documents in {0}.", TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("g"));
		}

		const string BulkImportSprocName = "BulkImport";

		[TestMethod]
		public void CreateStoredProcedure()
		{
			TryDeleteStoredProcedure(BulkImportSprocName).Wait();

			var markAntiquesSproc = new StoredProcedure
			{
				Id = BulkImportSprocName,
				Body = @"function bulkImport(docs) {
										var collection = getContext().getCollection();
										var collectionLink = collection.getSelfLink();

										// The count of imported docs, also used as current doc index.
										var count = 0;

										// Validate input.
										if (!docs) throw new Error(""The array is undefined or null."");

										var docsLength = docs.length;
										if (docsLength == 0) {
												getContext().getResponse().setBody(0);
										}

										// Call the create API to create a document.
										tryCreate(docs[count], callback);

										// Note that there are 2 exit conditions:
										// 1) The createDocument request was not accepted. 
										//    In this case the callback will not be called, we just call setBody and we are done.
										// 2) The callback was called docs.length times.
										//    In this case all documents were created and we don’t need to call tryCreate anymore. Just call setBody and we are done.
										function tryCreate(doc, callback) {
												var isAccepted = collection.createDocument(collectionLink, doc, callback);

												// If the request was accepted, callback will be called.
												// Otherwise report current count back to the client, 
												// which will call the script again with remaining set of docs.
												if (!isAccepted) getContext().getResponse().setBody(count);
										}

										// This is called when collection.createDocument is done in order to process the result.
										function callback(err, doc, options) {
												if (err) throw err;

												// One more document has been inserted, increment the count.
												count++;

												if (count >= docsLength) {
														// If we created all documents, we are done. Just set the response.
														getContext().getResponse().setBody(count);
												} else {
														// Create next document.
														tryCreate(docs[count], callback);
												}
										}
								}"
			};

			StoredProcedure createdStoredProcedure = docDBRepo
				.Client()
				.CreateStoredProcedureAsync(docDBRepo
					.Collection()
					.Result
					.SelfLink, markAntiquesSproc)
				.Result;

			Assert.IsNotNull(createdStoredProcedure);
			Assert.AreEqual(BulkImportSprocName, createdStoredProcedure.Id);
		}

		[TestMethod]
		public void DeleteAllDocuments()
		{
			docDBRepo.DeleteAllItemsAsync().Wait();
		}

		[TestMethod]
		public void BulkInsert()
		{
			const int batchSize = 50;
			int batchCounter = 0;

			// 504 documents to be inserted
			var documentProducts = FetchProductsFromSqlDatabaseAsync().Result.Cast<object>().ToArray();

			StoredProcedure storedProcedure = GetStoredProcedureAsync(BulkImportSprocName).Result;

			var stopwatch = new Stopwatch();
			int totalCount = 0;
			while (true)
			{
				var currentBatch = documentProducts.Skip(batchCounter * batchSize).Take(batchSize).ToArray();

				if (!currentBatch.Any()) break;

				var jsonArrayOfDocuments = ConvertToJSonArray(currentBatch);

				bool inserted = false;
				while (!inserted)
				{
					TimeSpan retryAfter = TimeSpan.Zero;

					try
					{
						stopwatch.Start();

						var storedProcedureResponse = docDBRepo
							.Client()
							.ExecuteStoredProcedureAsync<int>(storedProcedure.SelfLink, jsonArrayOfDocuments)
							.Result;

						stopwatch.Stop();

						var insertedDocsCount = storedProcedureResponse.Response;
						Assert.AreEqual(currentBatch.Count(), insertedDocsCount);
						ConsoleWrite("RequestCharge", storedProcedureResponse.RequestCharge);
						ConsoleWrite("Batch size: ", insertedDocsCount);

						batchCounter++;
						totalCount = totalCount + currentBatch.Count();
						inserted = true;
					}
					catch (AggregateException e)
					{
						var documentClientException = e.InnerExceptions.First() as DocumentClientException;
						if (documentClientException == null) throw;

						ConsoleWrite("StatusCode", documentClientException.StatusCode);
						ConsoleWrite("Error:", documentClientException.Error);
						retryAfter = documentClientException.RetryAfter;
						Console.WriteLine(e);
					}
					catch (DocumentClientException e)
					{
						ConsoleWrite("Error:", e.Error);
						retryAfter = e.RetryAfter;
						Console.WriteLine(e);
					}
					catch (Exception e)
					{
						ConsoleWrite("Exception:", e.GetType().FullName);
						ConsoleWrite("Message:", e.Message);
					}

					if (retryAfter != TimeSpan.Zero)
					{
						ConsoleWrite("RetryAfter: ", retryAfter.ToString("g"));
						Task.Delay(retryAfter).Wait();
						ConsoleWrite("Retrying...");
					}
				}
			}

			Assert.AreEqual(documentProducts.Length, totalCount);
			ConsoleWrite("Number of docs inserted: ", documentProducts.Length);
			ConsoleWrite("Total transaction clock time was: ", TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("g"));
		}

		[TestMethod]
		public void QueryData()
		{
			Assert.Fail("Not implemented");
		}

		[TestMethod]
		public void DeleteDatabase()
		{
			docDBRepo.Client().DeleteDatabaseAsync(docDBRepo.Database().Result.SelfLink);
		}

		private static async Task TryDeleteStoredProcedure(string sprocId)
		{
			StoredProcedure sproc = await GetStoredProcedureAsync(sprocId);
			if (sproc != null)
			{
				await docDBRepo.Client().DeleteStoredProcedureAsync(sproc.SelfLink);
			}
		}
		private static async Task<StoredProcedure> GetStoredProcedureAsync(string sprocId)
		{
			return await Task.Run(() => docDBRepo.Client().CreateStoredProcedureQuery(docDBRepo.Collection().Result.SelfLink).Where(s => s.Id == sprocId).AsEnumerable().FirstOrDefault());
		}

		private static dynamic[] ConvertToJSonArray(object[] currentBatch)
		{
			var argsJson = CreateBulkInsertScriptArguments(currentBatch);
			var args = new[] { JsonConvert.DeserializeObject<dynamic>(argsJson) };
			return args;
		}

		private static string CreateBulkInsertScriptArguments(object[] objectsToInsert)
		{
			var jsonDocumentArray = new StringBuilder();

			jsonDocumentArray.Append("[");

			var i = objectsToInsert.Count();
			foreach (var objectToInsert in objectsToInsert)
			{
				jsonDocumentArray.Append(JsonConvert.SerializeObject(objectToInsert));

				i--;

				if (i > 0)
				{
					jsonDocumentArray.AppendLine(", ");
				}
			}

			jsonDocumentArray.Append("]");

			return jsonDocumentArray.ToString();
		}

		/// <summary>
		/// This method will fetch data from the SQLDatabase using EntityFramework and convert the result to a much simpler document structure.
		/// </summary>
		private static async Task<IEnumerable<Product>> FetchProductsFromSqlDatabaseAsync(int? count = null)
		{
			var sqlDatabaseConnectionString = ConfigurationManager.ConnectionStrings["Azure SQL Database Demo"].ConnectionString;

			// Fetch data from our SQL Database.
			using (var entityConnection = new EntityConnection(sqlDatabaseConnectionString))
			using (var adventureWorksProductsEntities = new AdventureWorksProductsEntities(entityConnection))
			{
				IQueryable<SqlDatabase.Product> products = adventureWorksProductsEntities.Products;

				if (count.HasValue)
				{
					products = products.Take(count.Value);
					//dbSet = dbSet.Where(p=>p.ProductReviews.Any())
				}
				
				return await Task.Run(() => products
					// Convert the complex database model into a simpler document structure.
					.Select(Mapper.ToDocumentDatabaseProducts)
					.ToArray());
				}
		}

		private static void ConsoleWrite(string key, object value)
		{
			Console.WriteLine("{0}: {1}", key, value);
		}
		private static void ConsoleWrite(object value)
		{
			Console.WriteLine(value);
		}
	}
}