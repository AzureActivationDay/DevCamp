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
	 string endpointUrl = "";
     string authorizationKey = "";
    #endregion
        using (DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey))
        {
	     
           var colSelfLink = await GetCollectionSelfLink(client, "authors");
           var stephenKing = await client.CreateDocumentQuery<Author>(colSelfLink)
		   							.Where (author=>author.Name == "George" && author.Surname =="Martin")
									.AsDocumentQuery()
									.ExecuteNextAsync();
									
		  			
									//.AsEnumerable()
									//.FirstOrDefault ();
			stephenKing.Dump();
       }
	   
}


public class Author {
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