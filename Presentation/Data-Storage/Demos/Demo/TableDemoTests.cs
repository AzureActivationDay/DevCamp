using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Data_Storage_Demos
{
  public class PersonEntity : TableEntity
  {
    public PersonEntity(string lastName, string firstName)
    {
      PartitionKey = lastName;
      RowKey = firstName;
    }

    public PersonEntity() { }

    public string Country { get; set; }
  }

  public class AnimalEntity : TableEntity
  {
    public const string AnimalPartition = "Animals";

    public AnimalEntity(string name)
    {
      PartitionKey = AnimalPartition;
      RowKey = name;
    }

    public AnimalEntity() { }

    public string Color { get; set; }
  }

  [TestClass]
  public class TableDemoTests
  {
    private static CloudTable cloudTable;

    [ClassInitialize]
    public static void ConnectToStorage(TestContext context)
    {
      var connectionString =
        ConfigurationManager.ConnectionStrings["Azure Storage Account Demo Primary"].ConnectionString;

      CloudStorageAccount cloudStorageAccount;

      if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
      {
        Assert.Fail(
          "Expected connection string 'Azure Storage Account Demo Primary' to be a valid Azure Storage Connection String.");
      }

      var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
      cloudTable = cloudTableClient.GetTableReference("demotable");
      cloudTable.CreateIfNotExists();
    }

    const string LastName = "Martensson";
    const string FirstName = "Magnus";

    [TestMethod]
    public void SendEntityToTable()
    {
      var person = new PersonEntity(LastName, FirstName)
      {
        Country = "Sweden"
      };

      TableOperation insertOperation = TableOperation.InsertOrReplace(person);
      cloudTable.Execute(insertOperation);
    }

    [TestMethod]
    public void SendDifferentEntityToTable()
    {
      TableBatchOperation batchOperation = new TableBatchOperation();

      var dog = new AnimalEntity("Lassie")
      {
        Color = "Mixed"
      };
      var insertOperation = TableOperation.InsertOrReplace(dog);
      batchOperation.Add(insertOperation);

      dog = new AnimalEntity("Pluto")
      {
        Color = "Mixed"
      };
      insertOperation = TableOperation.InsertOrMerge(dog);
      batchOperation.Add(insertOperation);

      cloudTable.ExecuteBatch(batchOperation);
    }

    [TestMethod]
    public void ClearTable()
    {
      var getPersonOperation = TableOperation.Retrieve<PersonEntity>(LastName, FirstName);

      var tableResult = cloudTable.Execute(getPersonOperation);

      var person = (PersonEntity)tableResult.Result;

      if (person != null)
      {
        Assert.AreEqual(LastName, person.LastName());
        cloudTable.Execute(TableOperation.Delete(person));
      }

      TableQuery<AnimalEntity> animalsQuery = new TableQuery<AnimalEntity>()
        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, AnimalEntity.AnimalPartition));

      TableBatchOperation tableBatchOperation = new TableBatchOperation();
      foreach (AnimalEntity animalEntity in cloudTable.ExecuteQuery(animalsQuery))
      {
        var tableOperation = TableOperation.Delete(animalEntity);
        tableBatchOperation.Add(tableOperation);
      }

      if (tableBatchOperation.Any())
      {
        cloudTable.ExecuteBatch(tableBatchOperation);
      }
    }
  }

  public static class PersonEntityExtensions
  {
    public static string LastName(this PersonEntity personEntity)
    {
      return personEntity.PartitionKey;
    }
  }
}