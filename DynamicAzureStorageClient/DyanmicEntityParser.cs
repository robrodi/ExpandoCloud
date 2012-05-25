using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DynamicAzureStorageClient
{
    public class ExpandoEntity : DynamicObject
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

    }
    public class DyanmicEntityParser
    {
        private static readonly XNamespace AtomNs = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace DataServiceNs = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private static readonly XNamespace DataServiceMetadataNs = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        
        public IEnumerable<dynamic> ParseEntries(Stream content)
        {
            if (content == null) throw new ArgumentNullException("content");

            XDocument x = XDocument.Load(content);
            return x.Descendants(AtomNs + "entry").Select(entry => ParseEntry(entry)).Cast<dynamic>();
        }

        private static ExpandoObject ParseEntry(XElement entry)
        {
            var propertiesElement = entry.Descendants(DataServiceMetadataNs + "properties");
            var propertyNodes = propertiesElement.DescendantNodes().Where(x => x is XElement);
            var expandoEntity = new ExpandoObject();
            var dic = (IDictionary<string, object>)expandoEntity;
            foreach (XElement propertyNode in propertyNodes)
            {
                dic.Add(propertyNode.Name.LocalName, propertyNode.Value);
                Console.WriteLine();
            }
            return expandoEntity;
        }
    }
}