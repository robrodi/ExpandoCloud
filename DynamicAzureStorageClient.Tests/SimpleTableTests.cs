using System.Net.Http;
using Microsoft.WindowsAzure;
using NUnit.Framework;

namespace DynamicAzureStorageClient.Tests
{
    [TestFixture]
    public class SimpleTableTests
    {
        [Test]
        public void HelloWorld(){}
        [Test]
        public void ConnectToStorage()
        {
            var client = new DynamicStorageClient(CloudStorageAccount.DevelopmentStorageAccount);
            Assert.IsNotNull(client.GetEntities("JaggedTable1"));
        }
    }
}