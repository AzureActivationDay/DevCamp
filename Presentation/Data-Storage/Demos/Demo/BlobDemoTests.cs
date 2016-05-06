using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Data_Storage_Demos
{
	[TestClass]
	public class BlobDemoTests
	{
		static CloudBlobClient cloudBlobClient;

		[ClassInitialize]
		public static void ConnectToStorage(TestContext context)
		{
			var connectionString = ConfigurationManager.ConnectionStrings["Azure Storage Account Demo Primary"].ConnectionString;
			
			CloudStorageAccount cloudStorageAccount;
			
			if(!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
			{
				Assert.Fail("Expected connection string 'Azure Storage Account Demo Primary' to be a valid Azure Storage Connection String.");
			}
			
			cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
		}

		const string ContainerName = "blobdemocontainer";
		const string BlobName = "testblob.txt";
		const string SampleBlobContent = "This is the sample text which will be uploaded!";

		[TestMethod]
		public void UploadBlob()
		{
			var cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);

			// Note: This line of code does not always have to be executed when you 'know' the storage item exists.
			cloudBlobContainer.CreateIfNotExists();

			var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(BlobName);

			cloudBlockBlob.DeleteIfExists();

			Assert.IsFalse(cloudBlockBlob.Exists());

			cloudBlockBlob.UploadText(SampleBlobContent);

			Assert.IsTrue(cloudBlockBlob.Exists());
		}

		[TestMethod]
		public void DownloadBlob()
		{
			var cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);
			cloudBlobContainer.CreateIfNotExists();
		
			var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(BlobName);

			Assert.IsTrue(cloudBlockBlob.Exists());

			var blobContent = cloudBlockBlob.DownloadText();

			Assert.AreEqual(SampleBlobContent, blobContent);
		}

		[TestMethod]
		public void DeleteBlob()
		{
			// Note: Deleting (or modifying) data from Azure Storage is considered irreversible!

			var cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);

			if (!cloudBlobContainer.Exists()) return;

			var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(BlobName);

			if (cloudBlockBlob.Exists()) cloudBlockBlob.Delete();

			// Optionally delete the container too or simply delete the container with all it's blobs!
			cloudBlobContainer.Delete();
		}
	}
}