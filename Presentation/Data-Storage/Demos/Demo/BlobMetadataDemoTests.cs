using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable RedundantAssignment

namespace Data_Storage_Demos
{
	[TestClass]
	public class BlobMetadataDemoTests
	{
		const string ContainerName = "blobdemocontainer";
		const string BlobName = "testblob.txt";

		[ClassInitialize]
		public static void ConnectToStorage(TestContext context)
		{
			var connectionString = ConfigurationManager.ConnectionStrings["Azure Storage Account Demo Primary"].ConnectionString;
			
			CloudStorageAccount cloudStorageAccount;
			
			if(!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
			{
				Assert.Fail("Expected connection string 'Azure Storage Account Demo Primary' to be a valid Azure Storage Connection String.");
			}
			
			var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

			var cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);

			// Note: This line of code does not always have to be executed when you 'know' the storage item exists.
			cloudBlobContainer.CreateIfNotExists();
			cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(BlobName);
		}

		const string SampleMetadataKey = "this_is_metadata";

		static CloudBlockBlob cloudBlockBlob;

		const string SampleMetadataValue = "This works fine!";

		[TestMethod]
		public void UploadBlob()
		{
			cloudBlockBlob.DeleteIfExists();

			if (cloudBlockBlob.Metadata.ContainsKey(SampleMetadataKey))
			{
				cloudBlockBlob.Metadata.Remove(SampleMetadataKey);
			}

			var metadataValue = SampleMetadataValue;
			cloudBlockBlob.Metadata.Add(new KeyValuePair<string, string>(SampleMetadataKey, metadataValue));

			cloudBlockBlob.UploadText(string.Empty);

			Assert.IsTrue(cloudBlockBlob.Exists());
		}

		[TestMethod]
		public void DownloadBlobMetadata()
		{
			var expectedMetadataValue = SampleMetadataValue;

			// Note this call to .Exists() will in fact load the metadata on the blob from Azure Storage.
			Assert.IsTrue(cloudBlockBlob.Exists(), "Run the other test first to create the blob with metadata.");

			var metadatavalue = cloudBlockBlob.Metadata[SampleMetadataKey];

			Assert.AreEqual(expectedMetadataValue, metadatavalue);
		}

		const string SampleMetadataValueWithSwedishCharacters = "This contains Swedeish characters and must be encoded ÅÄÖ!";

		[TestMethod]
		public void UploadBlob_EncodedMetadata()
		{
			cloudBlockBlob.DeleteIfExists();

			if (cloudBlockBlob.Metadata.ContainsKey(SampleMetadataKey))
			{
				cloudBlockBlob.Metadata.Remove(SampleMetadataKey);
			}

			var metadataValue = SampleMetadataValueWithSwedishCharacters;
			var bytes = Encoding.UTF8.GetBytes(metadataValue);
			var base64MetadataValue = Convert.ToBase64String(bytes);
			cloudBlockBlob.Metadata.Add(new KeyValuePair<string, string>(SampleMetadataKey, base64MetadataValue));

			cloudBlockBlob.UploadText(string.Empty);

			Assert.IsTrue(cloudBlockBlob.Exists());
		}

		[TestMethod]
		public void DownloadBlob_EncodedMetadata()
		{
			var expectedMetadataValue = SampleMetadataValueWithSwedishCharacters;

			// Note this call to .Exists() will in fact load the metadata on the blob from Azure Storage.
			Assert.IsTrue(cloudBlockBlob.Exists(), "Run the other test first to create the blob with metadata.");

			var base64MetadataValue = cloudBlockBlob.Metadata[SampleMetadataKey];
			var bytes = Convert.FromBase64String(base64MetadataValue);
			var metadatavalue = Encoding.UTF8.GetString(bytes);

			Assert.AreEqual(expectedMetadataValue, metadatavalue);
		}
	}
}