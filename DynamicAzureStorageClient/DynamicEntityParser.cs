using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;

namespace DynamicAzureStorageClient
{
    public class DynamicEntityParser
    {
        private static Logger Logger = LogManager.GetLogger("DynamicEntityParser");

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
            Logger.Log(LogLevel.Info, "Parsing Entry with {0} Values", propertyNodes.Count());
            foreach (XElement propertyNode in propertyNodes)
            {
                string localName = propertyNode.Name.LocalName;
                var value = GetValue(propertyNode);
                expandoEntity[localName] = value;
                Logger.Log(LogLevel.Debug, "{0} : {1} ({2}) -> {3}", localName, value, value != null ? value.GetType().Name : "Null", expandoEntity[localName]);
            }

            return expandoEntity;
        }
        public static object GetValue(XElement propertyNode)
        {

            if (propertyNode == null)
                return null;
            var nullAttr = propertyNode.Attribute(DataServiceMetadataNs + "null");
            if (nullAttr != null && bool.Parse(nullAttr.Value)) return null;
            var typeAttr = propertyNode.Attribute(DataServiceMetadataNs + "type");
            string type = "string";

            if (typeAttr != null)
                type = typeAttr.Value;

            Func<string, object> parser;
            
            parser = GetParser(type);
            
            return parser.Invoke(propertyNode.Value);
        }

        private static Func<string, object> GetParser(string type)
        {
            switch (type)
            {
                case "Edm.Int32":
                    return value => int.Parse(value);
                case "Edm.DateTime":
                    return value => DateTime.Parse(value);
                case "Edm.Boolean":
                    return value => bool.Parse(value);
                case "Edm.Double":
                    return value => double.Parse(value);
                case "Edm.Guid":
                    return value => Guid.Parse(value);
                case "Edm.Int64":
                    return value => long.Parse(value);
                case "Edm.Binary":
                    return value => Convert.FromBase64String(value);
                default: // string or otherwise.
                    return value => value;
            }
        }
    }
}