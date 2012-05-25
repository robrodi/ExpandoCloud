using System;
using System.Diagnostics;
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
            AreEqual(SeedTable.CrapEntity1(), entities[0]);
            AreEqual(SeedTable.CrapEntity2(), entities[1]);
        }

        [Test]
        public void Perf()
        {
            DynamicEntityParser r = new DynamicEntityParser();
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(TestFilesPath + "SimpleResponse.xml");
            int iterations1 = 20;
            Stopwatch s = Stopwatch.StartNew();
            for (int i = 0; i < iterations1; i++)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var actual = r.ParseEntries(stream);
                foreach (var expandoEntity in actual)
                    expandoEntity.RowKey.ToString();
            }
            s.Stop();
            Console.WriteLine("{0} ms for {1} runs", s.ElapsedMilliseconds, iterations1);

            int iterations2 = 10000;
            s.Restart();
            for (int i = 0; i < iterations2; i++)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var actual = r.ParseEntries(stream);
                foreach (var expandoEntity in actual)
                    expandoEntity.RowKey.ToString();
            }

            s.Stop();
            Console.WriteLine("{0} ms for {1} runs", s.ElapsedMilliseconds, iterations2);
        }

        internal static void AreEqual(CrapEntity1 expected, dynamic actual)
        {
            Assert.IsNotNull(actual, "Null crap1");
            Assert.AreEqual(expected.PartitionKey, actual.PartitionKey, "PK");
            Assert.AreEqual(expected.RowKey, actual.RowKey, "PK");
            Assert.AreEqual(expected.Thing0, actual.Thing0, "T0");
            Assert.AreEqual(expected.Thing1, actual.Thing1, "T1");
            Assert.AreEqual(expected.Thing2, actual.Thing2, "T2");
            Assert.AreEqual(DateTime.Parse("2012-05-25T21:00:29.79Z"), actual.Timestamp, "Timestamp 1");
            // Dynamic Junk
            Assert.IsNull(actual["NotAField"]);
        }

        internal static void AreEqual(CrapEntity2 expected, dynamic actual)
        {
            Assert.IsNotNull(actual, "Null crap2");
            Assert.AreEqual(expected.PartitionKey, actual.PartitionKey, "PK");
            Assert.AreEqual(expected.RowKey, actual.RowKey, "PK");
            Assert.AreEqual(expected.Thing0, actual.Thing0, "T0");
            Assert.AreEqual(expected.Thing6, actual.Thing6, "T6");
            Assert.AreEqual(expected.Thing7, actual.Thing7, "T7");
            Assert.AreEqual(DateTime.Parse("2012-05-25T21:00:29.97Z"), actual.Timestamp, "Timestamp 2");
        }
    }
}

