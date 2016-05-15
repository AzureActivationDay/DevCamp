<Query Kind="Program">
  <NuGetReference>Microsoft.Azure.DocumentDB</NuGetReference>
  <NuGetReference>Microsoft.Azure.DocumentDB.TransientFaultHandling</NuGetReference>
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
           var colSelfLink = await GetCollectionSelfLink(client, "authorsWithChildren");
		   ////////////////////////////////////////////////WARNING/////////////////////////////
           //var collectionDeletedResponse=await client.DeleteDocumentCollectionAsync(colSelfLink);
		   //collectionDeletedResponse.Dump();
       }
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