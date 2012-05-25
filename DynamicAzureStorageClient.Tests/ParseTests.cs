using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
            DyanmicEntityParser r = new DyanmicEntityParser();
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(TestFilesPath + "SimpleResponse.xml");


            var actual = r.ParseEntries(stream);
            Assert.IsNotNull(actual);
            var entities = actual.ToArray();
            Assert.AreEqual(2, entities.Length);
            var first = entities[0];
            Assert.IsNotNull(first, "Null first");
            Assert.AreEqual(first.PartitionKey, "A");
        }
    }
}
