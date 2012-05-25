using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DynamicAzureStorageClient
{
    public class DyanmicEntityParser
    {
        private static readonly XNamespace AtomNs = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace DataServiceNs = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private static readonly XNamespace DataServiceMetadataNs = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";


        public IEnumerable<ExpandoEntity> ParseEntries(Stream content)
        {
            if (content == null) throw new ArgumentNullException("content");
            return XDocument.Load(content).Descendants(AtomNs + "entry").Select(ParseEntry);
        }

        private static ExpandoEntity ParseEntry(XContainer entry)
        {
            var propertiesElement = entry.Descendants(DataServiceMetadataNs + "properties");
            var propertyNodes = propertiesElement.DescendantNodes().Where(x => x is XElement);
            var expandoEntity = new ExpandoEntity();
            foreach (XElement propertyNode in propertyNodes)
                expandoEntity[propertyNode.Name.LocalName] = propertyNode.Value;
            return expandoEntity;
        }
    }
}