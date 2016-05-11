<Query Kind="Program">
  <NuGetReference>Microsoft.Azure.DocumentDB</NuGetReference>
  <NuGetReference>Microsoft.Azure.DocumentDB.TransientFaultHandling</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Microsoft.Azure.Documents</Namespace>
  <Namespace>Microsoft.Azure.Documents.Client</Namespace>
  <Namespace>Microsoft.Azure.Documents.Linq</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{

         #region "keys"
	 string endpointUrl = "https://azureactivationday2016.documents.azure.com:443/";
     string authorizationKey = "roLNv9S2f7T1Ur1eeDTKJoAOxqMhX7MbOgQJ1lt2ancrafSpYj87BpUfxBGZ6305mKCJsbKTTbBmU6Hvke6kVw==";
    #endregion
       using (DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey))
       {
	   
   	   var colSelfLink = await GetCollectionSelfLink(client, "authors");
       var storedProcedureSelfLink=await GetStoredProcedureSelfLink(client,colSelfLink,"bulkImport");
	   var authors=new Author[]{
			new Author { 
					id=Guid.NewGuid().ToString(),
					Name = "Dan", Surname = "Simmons",
					Books = new[] {
						new Book {Title = "Hyperion", Genre = "Science Fiction", Price = 34.56m, PublishedOn = new DateTime(1989,1,1)},
						new Book {Title = "Terror", Genre = "Thriller", Price = 23.45m, PublishedOn = new DateTime(2007,1,8)}
					}
				},
			new Author { 
					id=Guid.NewGuid().ToString(),
					Name = "John", Surname = "Tolkien",
					Books = new[] {
						new Book {Title = "Hobbit", Genre = "Fantasy", Price = 12.34m, PublishedOn = new DateTime(1937,9,21)}
					}
				},
			new Author { 
					id=Guid.NewGuid().ToString(),
					Name = "Stephen", Surname = "King",
					Books = new[] {
						new Book {Title = "The Stand", Genre = "Horror", Price = 12.34m, PublishedOn = new DateTime(1978,9,1)}
					}
				}
		};
		
		int currentCount = 0;
		int maxScriptSize = 50000;
		while (currentCount < authors.Count ())
       {
          
           string argsJson = CreateBulkInsertScriptArguments(authors, currentCount, authors.Count (), maxScriptSize);
           var args = new dynamic[] { JsonConvert.DeserializeObject<dynamic>(argsJson) };

           // 6. execute the batch.
           StoredProcedureResponse<int> scriptResult = await client.ExecuteStoredProcedureAsync<int>(storedProcedureSelfLink, args);

           // 7. Prepare for next batch.
           int currentlyInserted = scriptResult.Response;
           currentCount += currentlyInserted;
       }
  }
}

// Define other methods and classes here
public class Author {
	public string id { get; set; }
	public string Name {get; set;}
	public string Surname {get; set;}
	public Book[] Books {get; set;}
}
public class Book {
	public string Title {get; set;}
	public string Genre {get; set;}
	public DateTime? PublishedOn {get; set;}
	public decimal? Price {get; set;}
}


private static string CreateBulkInsertScriptArguments(Author[] authors, int currentIndex, int maxCount, int maxScriptSize)
   {
       var jsonDocumentArray = new StringBuilder();
       jsonDocumentArray.Append("[");

       if (currentIndex >= maxCount) return string.Empty;
       jsonDocumentArray.Append(JsonConvert.SerializeObject(authors[currentIndex]));

       int scriptCapacityRemaining = maxScriptSize;
       string separator = string.Empty;

       int i = 1;
       while (jsonDocumentArray.Length < scriptCapacityRemaining && (currentIndex + i) < maxCount)
       {
           jsonDocumentArray.Append(", " + JsonConvert.SerializeObject(authors[currentIndex + i]));
           i++;
       }

       jsonDocumentArray.Append("]");
       return jsonDocumentArray.ToString();
   }

private static async Task<string> GetStoredProcedureSelfLink(DocumentClient client,string colSelfLink,string id)
{
	  var storedProcedureList =  client.CreateStoredProcedureQuery(colSelfLink).Where (c => c.Id ==id).AsEnumerable().FirstOrDefault ();
      return storedProcedureList.SelfLink;
}

private static async Task<DocumentCollection> GetCollectionAsync(DocumentClient client, string dbLink, string id)
{
       DocumentCollection collection = client.CreateDocumentCollectionQuery(dbLink).Where(c => c.Id == id).ToArray().FirstOrDefault();
       return collection;
}
   


private static async Task<string> GetCollectionSelfLink(DocumentClient client, string id)
{
  var db = await GetDatabaseAsync(client, "bookStore");
  var collection = await GetCollectionAsync(client, db.SelfLink,id);
  return collection.SelfLink;
}

private static async Task<Database> GetDatabaseAsync(DocumentClient client, string id)
{
  Database database = client.CreateDatabaseQuery().Where(db => db.Id == id).ToArray().FirstOrDefault();
  return database;
}