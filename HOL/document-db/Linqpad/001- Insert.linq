<Query Kind="Program">
  <NuGetReference>Microsoft.Azure.DocumentDB</NuGetReference>
  <NuGetReference>Microsoft.Azure.DocumentDB.TransientFaultHandling</NuGetReference>
  <Namespace>Microsoft.Azure.Documents</Namespace>
  <Namespace>Microsoft.Azure.Documents.Client</Namespace>
  <Namespace>Microsoft.Azure.Documents.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
     #region "keys"
	 string endpointUrl = "";
     string authorizationKey = "";
    #endregion
	
      using (DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey))
       {
			var colSelfLink = await GetCollectionSelfLink(client, "authors");
			var createdDocument = await client.CreateDocumentAsync(colSelfLink, new Author
			{
				Name = "George",
				Surname = "Martin",
				Books = new[] {
				new Book {Title = "A Game of Thrones", Genre = "Fantasy", Price = 12.34m, PublishedOn = new DateTime(1996,8,6)},
				new Book {Title = "A Clash of Kings", Genre = "Fantasy", Price = 23.45m, PublishedOn = new DateTime(1998,11,16)}
			}
      });

      createdDocument.Dump();
  }
}

   // Define other methods and classes here
   public class Author
   {
       public string Name { get; set; }
       public string Surname { get; set; }
       public Book[] Books { get; set; }
   }
   public class Book
   {
       public string Title { get; set; }
       public string Genre { get; set; }
       public DateTime? PublishedOn { get; set; }
       public decimal? Price { get; set; }
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