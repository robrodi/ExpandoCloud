using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NLog;
using NUnit.Framework;

namespace DynamicAzureStorageClient.Tests
{
    [TestFixture]
    public class SimpleTableTests
    {
        private const string testTableName = "JaggedTable1";

        [Test]
        public void HelloWorld() { }
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

        [Test]
        public void PerfComp()
        {
            Logger l = LogManager.GetLogger("SimpleTableTests");

            l.Info("Warming up both.");
            l.Debug(GetWithTableServiceContext(CloudStorageAccount.DevelopmentStorageAccount).First().Thing0);
            l.Debug(GetWithDynamic().First().PartitionKey);
            l.Info("Done warming up both.");

            Stopwatch s = Stopwatch.StartNew();
            int numIterations = 100;
            for (int i = 0; i < numIterations; i++)
            {
                var results = GetWithTableServiceContext(CloudStorageAccount.DevelopmentStorageAccount);
                foreach (var crapEntity1 in results)
                    l.Debug("Static {0}ms : {1}", s.ElapsedMilliseconds, crapEntity1.Thing0);
            }
            s.Stop();
            l.Info("TableServiceContext: {0} iterations took {1} ms", numIterations, s.ElapsedMilliseconds);

            s.Restart();
            for (int i = 0; i < numIterations; i++)
            {
                var entities = GetWithDynamic();
                foreach (dynamic entity in entities)
                    l.Debug("Dynamic {0}ms : {1}", s.ElapsedMilliseconds, entity.Thing0);
            }
            s.Stop();
            l.Info("Dynamic Hotness: {0} iterations took {1} ms", numIterations, s.ElapsedMilliseconds);
        }

        private static IEnumerable<ExpandoEntity> GetWithDynamic()
        {
            var dynamicClient = new DynamicStorageClient(CloudStorageAccount.DevelopmentStorageAccount);
            var entities = dynamicClient.GetEntities(testTableName).Where(e => e.PartitionKey == "A" && e.RowKey == "1");
            return entities.ToArray();
        }

        private static CrapEntity1[] GetWithTableServiceContext(CloudStorageAccount account)
        {
            TableServiceContext context = new TableServiceContext(account.TableEndpoint.AbsoluteUri, account.Credentials);
            var query = context.CreateQuery<CrapEntity1>(testTableName);
            return (from entity in query
                    where entity.PartitionKey == "A" && entity.RowKey == "1"
                    select entity).AsTableServiceQuery().ToArray();
        }
    }
}