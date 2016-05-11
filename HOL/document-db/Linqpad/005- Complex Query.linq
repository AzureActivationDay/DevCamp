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
		   var fantasyAuthorsUsingSQLSyntax =  client.CreateDocumentQuery<dynamic>(colSelfLink,
			 						"select a.id,a.Name,b.Title,b.Genre from authors a "+
									"join b in a.Books "+
									"Where b.Genre = 'Fantasy'");
		   
		     fantasyAuthorsUsingSQLSyntax.Dump();
//           var fantasyAuthors =  client.CreateDocumentQuery<Author>(colSelfLink)
//		   						 .SelectMany (author =>author.Books
//								 .SelectMany (book =>author.Books
//								 .Where (book1 =>book1.Genre =="Fantasy")
//								 .Select (authorFields =>new
//								 {
//								 	  id=author.id,
//									  Name=author.Name,
//									  Title=book.Title,
//									  Genre=book.Genre
//								 })))
//								 .AsEnumerable()
//								 .Distinct();
//		   fantacyAuthors.Dump();
			
       }

	
	
}

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