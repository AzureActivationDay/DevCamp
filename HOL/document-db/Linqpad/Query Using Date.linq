<Query Kind="Program">
  <NuGetReference>Microsoft.Azure.DocumentDB</NuGetReference>
  <NuGetReference>Microsoft.Azure.DocumentDB.TransientFaultHandling</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Microsoft.Azure.Documents</Namespace>
  <Namespace>Microsoft.Azure.Documents.Client</Namespace>
  <Namespace>Microsoft.Azure.Documents.Client.TransientFaultHandling</Namespace>
  <Namespace>Microsoft.Azure.Documents.Linq</Namespace>
  <Namespace>Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	   
	   var date="1996-08-06T00:00:00";
	   
        #region "keys"
	 string endpointUrl = "";
     string authorizationKey = "";
    #endregion
        using (DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey))
        {
	     
           var colSelfLink = await GetCollectionSelfLink(client, "authors");
		   string sql="select c.Name,c.Surname, d.Title,d.Genre,d.PublishedOn,d.Price from c join d in c.Books where d.PublishedOn = '" + date + "'";
           var output =  client.CreateDocumentQuery<dynamic>(colSelfLink,sql)
		  							.AsEnumerable();
			
			output.Dump();
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

  public static int ToEpoch(DateTime date)
    {
        if (date == null) return int.MinValue;
        DateTime epoch = new DateTime(1970, 1, 1);
        TimeSpan epochTimeSpan = date - epoch;
        return (int)epochTimeSpan.TotalSeconds;
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