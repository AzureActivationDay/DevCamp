# Microsoft Azure Storage Demos #

<a name="Overview"></a>
## Overview ##
This is a set of demos showing how to interact with [Microsft Azure Storage](http://azure.microsoft.com/en-us/documentation/services/storage/) using tools, http and C# code.

<a id="goals" />
### Goals ###
In these demos, you will see how to:

1. Use a tool to interact with Azure Storage.
2. Leverage the [C# Azure Storage library](https://www.nuget.org/packages/WindowsAzure.Storage) to interact with Azure storage in code.

<a name="setup"></a>
### Setup and Configuration ###
Follow these steps to setup your environment for the demo.

1. Sign into [Microsoft Azure Portal](http://portal.azure.com) and create a demo Storage account.
2. Rename the template configuration file 'SharedConfiguration.original.config' -> SharedConfiguration.config and configure the "Azure Storage Account Demo Primary" connection string with they account name and key from your new storage account.
3. Open the demo solution "Demos\Data-Storage-Demos.sln"" in Visual Studio and review the code demos. They are structured in once Unit Test class per demo. The general idea is that you walk through the code in your demo and also execute the unit tests from top to bottom in the classes!
4. Also go through the steps on the tooling and interaction demos below!
5) Download a third party Storage Explorer tool of your choice from this list: [Azure Storage Explorers](http://blogs.msdn.com/b/windowsazurestorage/archive/2014/03/11/windows-azure-storage-explorers-2014.aspx)
6) Connect to your Azure Storage demo account using the third party tool.

<a name="Demo1"></a>
## Demo 1) Interact with Azure Storage Blobs ##

1. Using the [Azure Storage Explorer](http://storageexplorer.com/) tool create a container in your Azure Storage account.
2. Upload a file for instance an image. (It can be any image you feel like sharing.)
3. Copy the URL to the file and browse to it. It does not work.
4. Change the container access permissions to allow public read access (at the container level or at the blob level).5) Try browsing to the blob again. It works.
5. Upload a small video file, copy the URL to it and open it in Media Player. Streaming of the blob should happen. (Azure Storage supports smooth streaming.)
6. Using this storage explorer tool, you can as well demonstrate how easy it is to copy blobs between two different storage accounts. It involved multiple steps in the PowerShell world.

> **Speaking point:** Explain that interacting with Storage Blobs is really easy using a tool. Any tool that can make a call over http can access your files in Azure Storage. There is also a CDN (Content Delivery Network) Service which can be enabled for Azure Storage which effectively caches your files in edge nodes closer to the downloading users around the globe. 
	> Note: Azure Storage Explorer is a tool released by Microsoft and is currently under version 0.7.

<a name="Demo2"></a>
## Demo 2) Interact with Azure Storage Blobs through code ##

* In the Demo solution open the class BlobDemoTests. It contains Unit Tests which are supposed to be executed sequentially while showing the code. You can even choose to debug the tests and step through them wile explaining to the audience.
* Execute the unit tests in the order top to bottom: 
    * **.UploadBlob()** - connects with Storage, creates a container and uploads a text into a blob in the container.
    * **.DownloadBlob()** - connects with storage and downloads the same blob.
    * **.DeleteBlob()** - connects with storage and deletes the blob.

**Tip:** You can also show the blob in a storage explorer as it is created!

<a name="Demo3"></a>
## Demo 3) Blob metadata ##

This test class has four methods. The first two will upload a blob with metadata and download it again. The second two do the same only the metadata contains invalid characters which means we have to base64 encode the metadata value before uploading it.

* **.UploadBlob()** - Upload a blob including some metadata.
    * **Tip:** Look at the blob metadata in your Storage Explorer.
* **. DownloadBlobMetadata()** - Download the metadata again.
* **.UploadBlob_EncodedMetadata()** - Because this other metadata has invalid characters it has to be stored as base64 encoded data.
    * **Tip:** Look at the blob metadata in your Storage Explorer. The challenge here becomes that the metadata is not human readable in an easy way. Still the metadata is storable so there is a trade off.
* **.DownloadBlob_EncodedMetadata()** - Download and decode the metadata from the upload above.

> **Speaking Point:** The metadata is not searchable using the API. The total size of the metadata, including both the name and value together, may not exceed 8 KB in size.

<a name="Demo4"></a>
## Demo 4) Shared Access Signatures (SAS) ##

The class SharedAccessSignaturesDemoTests contains two methods that demo SAS access to Azure Storage.

* **.UploadBlob_using_one_time_SAS()** - Creates a custom Shared Access Signature to a specific blob with write only access. Uses it to upload a blob.
* **.DownloadBlob_using_SharedAccessPolicies()** - Creates a Shared Access Policy for reading blobs in a container and uploads it to Azure Storage. Using this policy a SAS key is created and is used to download the blob. The policy is then expired and the URL is not usable. After the policy is prolonged the same URL can again access the blob.
    * **Tip:** Look at the blob metadata in your Storage Explorer.

> **Speaking Point:** An ordinary SAS key can be used for a specific blob target with specific access rights and time. A Stored Access Policy is persisted in an Azure Storage container and can be revoked on the server side. Two complimentary and great features that provides flexible and fine grained access to 
Azure Storage.

<a name="Demo5"></a>
## Demo 5) Queues in Code ##

In this demo a queue message will be sent to a queue, it will be updated and then collected from the queue.

* **.SendMessageToQueue()** - This method sends a simple message to an Azure Queue.
    * **Tip:** You can peek into the queue and see the message using a Storage Explorer tool.
* **.ModifyMessageInQueue()** - This method gets a message from a queue, modifies the message and updates back into the queue.
    * **Tip:** You can peek into the queue and see the message using a Storage Explorer tool.
* **.GetMessageFromQueue()** - This method collects the updated message from the queue and then confirms the dequeue by deleting it.
* **.ClearMessages()** - This method clears all messages from the queue in case you need to start over.

> **Speaking Points:** A message body of a queue message can be maximum 64KB in size. UpdateMessage can be used to process one message in several steps but normal processing is to send and receive messages only once. The delete message action confirms to the queue that the message is indeed processed. If the message is not deleted in time the message will reappear in the queue with the DequeueCount property raised by one. This enables poison message handling. The queue timeout time for holding on to a message before it reappears is a value you can set when you fetch the message. Queues do not support batch processing.

<a name="Demo6"></a>
## Demo 6) Enter Entities into a table ##

The demo class TableDemoTests contains three tests that interact with Azure Tables.

* **.SendEntityToTable()** - The method inserts or replaces a simple PersonEntity into the Azure Table.
    * **Tip:** You can see the Entity in the table using a Storage Explorer tool.
* **.SendDifferentEntityToTable()** - The method batch inserts two AnimalEntities into the same Azure Table.
    * **Tip:** You can see the Entities in the table using a Storage Explorer tool.
* **.ClearTable()** - The method removes first the person and then the two dogs.

> **Speaking Points:** InsertOrReplace and InsertOrMerge are useful operations on Azure Tables which are more mild than Insert. A Table can have 255 properties three of which are PartitionKey, RowKey and TimeStamp. The first two make up the composite unique key for the row. Batching operations can take place on one partition only. Rows with the same PartitionKey are always stored on one storage node and are fast to interact with. Strive to use the PartitionKey in every query on an Azure Table else a fan-out-query might be executed in the storage system. A table does not enforce a schema - in our sample Persons and Animals live happily in the same table. Normal operations is to have one type of entity in a table but different versions of the entity might define different columns.

<a name="summary"></a>
## Summary ##

By completing these demos you should have understood how you can leverage Microsoft Azure Storage for your storage needs and access it from your code.
