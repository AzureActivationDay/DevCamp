using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Data_Platform_Demos.DocumentDB
{
	public class DocumentDBRepository<T>
	{
		private readonly Uri endpoint;
		private readonly string authKey;
		private readonly string databaseId;
		private readonly string collectionId;

		private DocumentClient client;
		private readonly string collectionsLink;

		private readonly string documentCollectionSelfLink;
		private readonly string documentsLink;

		public DocumentDBRepository(string endpoint, string authKey, string databaseId, string collectionId)
		{
			this.endpoint = new Uri(endpoint);
			this.authKey = authKey;
			this.databaseId = databaseId;
			this.collectionId = collectionId;

			// Create Database if not exists
			var documentClient = Client();
			var db = documentClient.CreateDatabaseQuery().Where(d => d.Id == databaseId).AsEnumerable().FirstOrDefault() ??
							 documentClient.CreateDatabaseAsync(new Database { Id = databaseId }).Result;

			collectionsLink = db.CollectionsLink;

			// Create Collection if not exists
			var documentCollection = documentClient.CreateDocumentCollectionQuery(collectionsLink).Where(c => c.Id == collectionId).AsEnumerable().FirstOrDefault() ?? 
				documentClient.CreateDocumentCollectionAsync(collectionsLink, new DocumentCollection { Id = collectionId }).Result;

			documentsLink = documentCollection.DocumentsLink;
			documentCollectionSelfLink = documentCollection.SelfLink;
		}

		public DocumentClient Client(ConsistencyLevel? consistencyLevel = ConsistencyLevel.Session)
		{
			if (client != null && client.ConsistencyLevel == consistencyLevel)
			{
				return client;
			}
			client = new DocumentClient(endpoint, authKey, null, consistencyLevel);
			return client;
		}

		public async Task<Database> Database(ConsistencyLevel? consistencyLevel = null)
		{
			return await Task.Run((() => Client(consistencyLevel)
				.CreateDatabaseQuery()
				.Where(d => d.Id == databaseId)
				.AsEnumerable()
				.First()));
		}

		public async Task<DocumentCollection> Collection(ConsistencyLevel? consistencyLevel = null)
		{
			return await Task.Run(() => Client(consistencyLevel)
				.CreateDocumentCollectionQuery(collectionsLink)
				.Where(c => c.Id == collectionId)
				.AsEnumerable()
				.First());
		}

		public async Task<IEnumerable<T>> GetItemsAsync(Func<T, bool> clause, ConsistencyLevel? consistencyLevel)
		{
			return (await Task.Run(() =>
			{
				return Client(consistencyLevel)
					.CreateDocumentQuery<T>(documentsLink)
					.Where(d => clause(d))
					.AsEnumerable().ToList();
			})).AsReadOnly();
		}

		public async Task<Document> CreateDocumentAsync(T item, ConsistencyLevel? consistencyLevel)
		{
			var resourceResponse = await Client(consistencyLevel).CreateDocumentAsync(documentCollectionSelfLink, item, null, true);
			return resourceResponse;
		}

		public async Task<T> GetItemAsync(string id, Func<T, bool> clause, ConsistencyLevel? consistencyLevel)
		{
			return await Task.Run(() => Client(consistencyLevel)
				.CreateDocumentQuery<T>(documentsLink)
				.Where(d => clause(d))
				.AsEnumerable()
				.FirstOrDefault());
		}

		public async Task<Document> GetDocumentAsync(string id, ConsistencyLevel? consistencyLevel)
		{
			var document = await Task.Run(() => Client(consistencyLevel)
				.CreateDocumentQuery(documentsLink)
				.Where(d => d.Id == id)
				.AsEnumerable()
				.FirstOrDefault());
			return document;
		}

		public async Task<Document> UpdateDocumentAsync(T item, Func<T, string> idFunc, ConsistencyLevel? consistencyLevel = null)
		{
			var document = await GetDocumentAsync(idFunc(item), consistencyLevel);
			if (document == null)
			{
				return await CreateDocumentAsync(item, consistencyLevel);
			}
			var resourceResponse = await Client(consistencyLevel).ReplaceDocumentAsync(document.SelfLink, item);
			return resourceResponse;
		}

		public async Task DeleteItemAsync(string id, ConsistencyLevel? consistencyLevel = null)
		{
			var document = await Task.Run(() => Client(consistencyLevel)
				.CreateDocumentQuery(documentsLink)
				.Where(d => d.Id == id)
				.AsEnumerable()
				.FirstOrDefault());

			if (document != null)
			{
				await Client(consistencyLevel).DeleteDocumentAsync(document.SelfLink);
			}
		}

		public async Task DeleteAllItemsAsync(ConsistencyLevel? consistencyLevel = null)
		{
			var documents = await Task.Run(() => Client(consistencyLevel)
				.CreateDocumentQuery(documentsLink)
				.AsEnumerable());

			foreach (var document in documents)
			{
				await DeleteItemAsync(document.Id);
			}
		}
	}
}