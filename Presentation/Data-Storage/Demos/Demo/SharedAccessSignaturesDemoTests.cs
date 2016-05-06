using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Data_Storage_Demos
{
  [TestClass]
  public class SharedAccessSignaturesDemoTests
  {
    const string ContainerName = "blobdemocontainer2";
    const string BlobName = "testblob.txt";
    const string SampleBlobContent = "This is the sample text which will be uploaded to our SAS blob!";

    static CloudBlobClient cloudBlobClient;
    static CloudBlobContainer cloudBlobContainer;

    [ClassInitialize]
    public static void ConnectToStorage(TestContext context)
    {
      var connectionString = ConfigurationManager.ConnectionStrings["Azure Storage Account Demo Primary"].ConnectionString;

      CloudStorageAccount cloudStorageAccount;

      if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
      {
        Assert.Fail("Expected connection string 'Azure Storage Account Demo Primary' to be a valid Azure Storage Connection String.");
      }

      cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
      cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);
      cloudBlobContainer.CreateIfNotExists();
    }

    [TestMethod]
    public void UploadBlob_using_one_time_SAS()
    {
      var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(BlobName);
      cloudBlockBlob.DeleteIfExists();

      var write5SecSAS = cloudBlockBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy
      {
        Permissions = SharedAccessBlobPermissions.Write,
        SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(5)
      });

      var blobWriteSasUri = new Uri(cloudBlockBlob.Uri.AbsoluteUri + write5SecSAS);

      // Here we are write only accessing the blob based on a SAS uri only!
      var sasAccessedBlob = new CloudBlockBlob(blobWriteSasUri);
      sasAccessedBlob.UploadText(SampleBlobContent);

      Assert.IsTrue(cloudBlockBlob.Exists());
    }

    [TestMethod]
    public void DownloadBlob_using_SharedAccessPolicies()
    {
      // In this unit test demo there is a timing aspect below where a policy expires.
      // If run in debug the test might fail

      var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(BlobName);
      Assert.IsTrue(cloudBlockBlob.Exists(), "If the blob does not exist then execute the test above!");

      const string storedPolicyKey = "demostoredpolicy";

      // 1) Set the policy in Azure Storage on the container.

      // Make sure our policy does not already exist.
      var blobContainerPermissions = cloudBlobContainer.GetPermissions();
      if (blobContainerPermissions.SharedAccessPolicies.ContainsKey(storedPolicyKey))
      {
        blobContainerPermissions.SharedAccessPolicies.Remove(storedPolicyKey);
      }
      // Or use blobContainerPermissions.SharedAccessPolicies.Clear();

      // Set our access policy in Azure to grant read access for a short time given the key.
      blobContainerPermissions.SharedAccessPolicies.Add(
        storedPolicyKey,
        new SharedAccessBlobPolicy
        {
          Permissions = SharedAccessBlobPermissions.Read,
          SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(5)
        });
      cloudBlobContainer.SetPermissions(blobContainerPermissions);

      // 2) Use the policy only to 

      // This is the information a server would send to a client: Blob Uri including the SAS key!
      var blobAbsoluteUri = new Uri(cloudBlockBlob.Uri.AbsoluteUri);
      var sharedAccessSignature = cloudBlockBlob.GetSharedAccessSignature(null, storedPolicyKey);
      var blobAccess = blobAbsoluteUri + sharedAccessSignature;

      // Use the named key to get access
      // This is typically done on a client that wants access using the policy they have been given
      var blobAccessUri = new Uri(blobAccess);
      var blobAccessBySAS = new CloudBlockBlob(blobAccessUri);
      // Here we use the access by being able to read that the blob does indeed exist
      Assert.IsTrue(blobAccessBySAS.Exists());

      // Allow the policy to expire on the server
      Task.Delay(TimeSpan.FromSeconds(5)).Wait();
      try
      {
        blobAccessBySAS.Exists();

        Assert.Fail("The policy should have been expired!");
      }
      catch (StorageException sex)
      {
        // We have no access to the blob on an expired policy. This is forbidden!
        const HttpStatusCode expectedStatusCode = HttpStatusCode.Forbidden;
        var returnedStatusCode = (HttpStatusCode)sex.RequestInformation.HttpStatusCode;
        Assert.AreEqual(expectedStatusCode, returnedStatusCode);
      }

      // Prolong the policy on the server
      blobContainerPermissions.SharedAccessPolicies[storedPolicyKey].SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(5);
      cloudBlobContainer.SetPermissions(blobContainerPermissions);

      // Access is again granted by the same stored policy as before! We did not need to send a new url to the client!
      Assert.IsTrue(blobAccessBySAS.Exists());
    }

    [TestMethod]
    public void DeleteContainer()
    {
      // Note: Deleting (or modifying) data from Azure Storage is considered irreversible!
      cloudBlobClient.GetContainerReference(ContainerName).DeleteIfExists();
    }
  }
}