using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;
using NUnit.Framework;

namespace DynamicAzureStorageClient.Tests
{
    [TestFixture]
    public class ParseTests
    {
        public const string ResourcePrefix = "DynamicAzureStorageClient.Tests.";
        public const string TestFilesPath = ResourcePrefix + "TestFiles.";
        [Test]
        public void ParseSimpleObject()
        {
            string content;
            DynamicEntityParser r = new DynamicEntityParser();
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(TestFilesPath + "SimpleResponse.xml");


            var actual = r.ParseEntries(stream);
            Assert.IsNotNull(actual);
            var entities = actual.ToArray();
            Assert.AreEqual(2, entities.Length);
            dynamic first = entities[0];
            Assert.IsNotNull(first, "Null first");
            Assert.AreEqual("A", first.PartitionKey, "PK");
            Assert.AreEqual("1", first.RowKey, "RK");
            Assert.AreEqual(1234, first.Thing2, "Thing 2");
            Assert.AreEqual(DateTime.Parse("2012-05-25T00:48:59.16Z"), first.Timestamp, "Timestamp");
        }
    }
}

