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
	   
   	   var colSelfLink = await GetCollectionSelfLink(client, "authorsWithChildren");
       var author1=	new Author { 
					id=Guid.NewGuid().ToString(),
					Name = "Donald", Surname = "Jones",
					Books = new[] {
						new Book {Title = "How to save the world in 2 minutes?", Genre = "Fiction", Price = 354.56m, PublishedOn = new DateTime(2015,1,1)}
					},
					Children = new Child[] {
                    new Child
                    { 
                        FirstName = "Joanne", 
                        Gender = "female", 
                        Grade = 5, 
                        Pets = new [] {
                            new Pet { GivenName = "Neo" } 
                        }
                    } 
                },
                Address = new Address { State = "VIC",  City = "Melbourne" },
				};
		
		var createdDocument1 = await client.CreateDocumentAsync(colSelfLink,author1);
		createdDocument1.Dump();
				
		var author2=new Author { 
					id=Guid.NewGuid().ToString(),
					Name = "John", Surname = "Steele",
					Books = new[] {
						new Book {Title = "Not that hobbit", Genre = "Fantasy", Price = 12.34m, PublishedOn = new DateTime(2014,9,21)}
					},
					Children = new Child[] {
                    new Child
                    { 
                        FirstName = "Michael", 
                        Gender = "Male", 
                        Grade = 3, 
                        Pets = new [] {
                            new Pet { GivenName = "Humphrey" } 
                        }
                    } 
                },
                Address = new Address { State = "NSW",  City = "Sydney" },
				};
				
		var createdDocument2 = await client.CreateDocumentAsync(colSelfLink,author2);
		createdDocument2.Dump();
				
				
		var author3=new Author { 
					id=Guid.NewGuid().ToString(),
					Name = "Stephen", Surname = "King",
					Books = new[] {
						new Book {Title = "The Grand stand", Genre = "Horror", Price = 12.34m, PublishedOn = new DateTime(2015,9,1)}
					},
					Children = new Child[] {
                    new Child
                    { 
                        FirstName = "Kate", 
                        Gender = "female", 
                        Grade = 5, 
                        Pets = new [] {
                            new Pet { GivenName = "Fluffy" } 
                        }
                    } 
                },
                Address = new Address { State = "SA",  City = "Adelaide" },
				};
	
	var createdDocument3 = await client.CreateDocumentAsync(colSelfLink,author3);
	createdDocument3.Dump();
		
		
  }
}

// Define other methods and classes here
public class Author {
	public string id { get; set; }
	public string Name {get; set;}
	public string Surname {get; set;}
	public Book[] Books {get; set;}
	public Address Address { get; set; }
	public Child[] Children { get; set; }
}
public class Book {
	public string Title {get; set;}
	public string Genre {get; set;}
	public DateTime? PublishedOn {get; set;}
	public decimal? Price {get; set;}
}

public class Child
{
    public string FamilyName { get; set; }
    public string FirstName { get; set; }
  	public string Gender { get; set; }
  	public int Grade { get; set; }
  	public Pet[] Pets { get; set; }
}

public class Address
{
  public string State { get; set; }
  public string City { get; set; }
}

public class Pet
{
  public string GivenName { get; set; }
}


   private static async Task<DocumentCollection> GetOrCreateCollectionAsync(DocumentClient client,string dbLink, string id)
   {
       DocumentCollection collection = client.CreateDocumentCollectionQuery(dbLink).Where(c => c.Id == id).ToArray().FirstOrDefault();

       if (collection == null)
       {
           IndexingPolicy optimalQueriesIndexingPolicy = new IndexingPolicy();
           optimalQueriesIndexingPolicy.IncludedPaths.Add(new IncludedPath
           {
               Path = "/*",
               Indexes = new System.Collections.ObjectModel.Collection<Index>()
               {
                   new RangeIndex(DataType.Number) { Precision = -1 },
                   new RangeIndex(DataType.String) { Precision = -1 }
               }
           });

           DocumentCollection collectionDefinition = new DocumentCollection { Id = id };
           collectionDefinition.IndexingPolicy = optimalQueriesIndexingPolicy;

           collection = await client.CreateDocumentCollectionAsync(dbLink, collectionDefinition);
       }

       return collection;
   }

//private static async Task<DocumentCollection> GetCollectionAsync(DocumentClient client, string dbLink, string id)
//{
//       DocumentCollection collection = client.CreateDocumentCollectionQuery(dbLink).Where(c => c.Id == id).ToArray().FirstOrDefault();
//       return collection;
//}
   


private static async Task<string> GetCollectionSelfLink(DocumentClient client, string id)
{
  var db = await GetDatabaseAsync(client, "bookStore");
  var collection = await GetOrCreateCollectionAsync(client, db.SelfLink,id);
  return collection.SelfLink;
}

private static async Task<Database> GetDatabaseAsync(DocumentClient client, string id)
{
  Database database = client.CreateDatabaseQuery().Where(db => db.Id == id).ToArray().FirstOrDefault();
  return database;
}