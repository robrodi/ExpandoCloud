using System.Linq;
using System.Net.Http;
using Microsoft.WindowsAzure;
using NUnit.Framework;

namespace DynamicAzureStorageClient.Tests
{
    [TestFixture]
    public class SimpleTableTests
    {
        private const string testTableName = "JaggedTable1";

        [Test]
        public void HelloWorld(){}
        [Test]
        public void ConnectToStorage()
        {
            var client = new DynamicStorageClient(CloudStorageAccount.DevelopmentStorageAccount);
            Assert.IsNotNull(client.GetStreams(testTableName));
        }

        [Test]
        public void GetEntitiesFromStorage()
        {
            var client = new DynamicStorageClient(CloudStorageAccount.DevelopmentStorageAccount);
            var entities = client.GetEntities(testTableName).ToArray();
            Assert.IsNotNull(entities);
            Assert.AreEqual(2, entities.Count());
            ParseTests.AreEqual(SeedTable.CrapEntity1(), entities[0]);
            ParseTests.AreEqual(SeedTable.CrapEntity2(), entities[1]);
        }

        [Test]
        public void GetByPartitionKey()
        {
            var client = new DynamicStorageClient(CloudStorageAccount.DevelopmentStorageAccount);
            var entities = client.GetStreams(testTableName, "A");
            Assert.IsNotNull(entities);
        }
    }
}