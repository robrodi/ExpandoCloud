using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

namespace DynamicAzureStorageClient.Tests
{
    public class CrapEntity1 : TableServiceEntity
    {
        public CrapEntity1()
        {
            // hooray
        }

        public string Thing0 { get; set; }
        public string Thing1 { get; set; }
        public int Thing2 { get; set; }
        //public float Thing3 { get; set; }
        //public byte Thing4 { get; set; }
        //public byte[] Thing5 { get; set; }
    }
    public class CrapEntity2 : TableServiceEntity
    {
        public string Thing0 { get; set; } // one shared 5 different.
        public string Thing6 { get; set; }
        public int Thing7 { get; set; }
    }


    public class SeedTable
    {
        [Test]
        public void CreateJaggedTable()
        {
            string tableName = "JaggedTable1";
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient client = new CloudTableClient(account.TableEndpoint.AbsoluteUri, account.Credentials);
            client.DeleteTableIfExist(tableName);
            client.CreateTableIfNotExist(tableName);
            TableServiceContext context = new TableServiceContext(account.TableEndpoint.AbsoluteUri, account.Credentials);
            
            var crapEntity1 = CrapEntity1();
            var crapEntity2 = CrapEntity2();
            context.AddObject(tableName, crapEntity1);
            context.SaveChanges();
            context.AddObject(tableName, crapEntity2);
            context.SaveChanges();
        }

        public static CrapEntity2 CrapEntity2()
        {
            return new CrapEntity2 { PartitionKey = "A", RowKey = "2", Thing0 = "E1", Thing6 = "Hi", Thing7 = 1234};
        }

        public static CrapEntity1 CrapEntity1()
        {
            return new CrapEntity1 { PartitionKey = "A", RowKey = "1", Thing0 = "E1", Thing1 = "Hi", Thing2 = 1234};
        }
    }
}
