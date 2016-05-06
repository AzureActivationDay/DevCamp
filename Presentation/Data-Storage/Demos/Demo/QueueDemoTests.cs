using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Data_Storage_Demos
{
  [TestClass]
  public class QueueDemoTests
  {
    static CloudQueue cloudQueue;
 
    [ClassInitialize]
    public static void ConnectToStorage(TestContext context)
    {
      var connectionString = ConfigurationManager.ConnectionStrings["Azure Storage Account Demo Primary"].ConnectionString;

      CloudStorageAccount cloudStorageAccount;

      if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
      {
        Assert.Fail("Expected connection string 'Azure Storage Account Demo Primary' to be a valid Azure Storage Connection String.");
      }

      var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
      cloudQueue = cloudQueueClient.GetQueueReference("demoqueue");

      // Note: Usually this statement can be executed once during application startup or maybe even never in the application.
      //       A queue in Azure Storage is often considered a persistent item which exists over a long time.
      //       Every time .CreateIfNotExists() is executed a storage transaction and a bit of latency for the call occurs.
      cloudQueue.CreateIfNotExists();
    }

    const string MessageText = "Here is a nice message!";

    [TestMethod]
    public void SendMessageToQueue()
    {
      var message = new CloudQueueMessage(MessageText);

      cloudQueue.AddMessage(message);
    }

    const string AdditionalMessage = " And some more message!";

    [TestMethod]
    public void ModifyMessageInQueue()
    {
      var cloudQueueMessage = cloudQueue.GetMessage(TimeSpan.FromSeconds(2));

      var message = cloudQueueMessage.AsString;

      Assert.AreEqual(MessageText, message);
      Assert.AreEqual(1, cloudQueueMessage.DequeueCount);

      message = message + AdditionalMessage;

      cloudQueueMessage.SetMessageContent(message);
      cloudQueue.UpdateMessage(cloudQueueMessage, TimeSpan.FromSeconds(1), MessageUpdateFields.Visibility | MessageUpdateFields.Content);
    }

    [TestMethod]
    public void GetMessageFromQueue()
    {
      var cloudQueueMessage = cloudQueue.GetMessage();

      if (cloudQueueMessage == null)
      {
        return;
      }

      Assert.AreEqual(MessageText + AdditionalMessage, cloudQueueMessage.AsString);
      Assert.AreEqual(2, cloudQueueMessage.DequeueCount);

      cloudQueue.DeleteMessage(cloudQueueMessage);
    }

    [TestMethod]
    public void ClearMessages()
    {
      cloudQueue.Clear();
    }
  }
}