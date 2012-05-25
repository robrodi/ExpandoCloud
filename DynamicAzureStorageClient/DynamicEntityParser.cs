using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using ValueParser = System.Func<string, object>;
namespace DynamicAzureStorageClient
{
    public interface IParser
    {
        /// <summary>
        /// Parses the entries in the XML Stream.
        /// </summary>
        /// <param name="content">The Xml content.</param>
        /// <returns>The entities.</returns>
        IEnumerable<ExpandoEntity> ParseEntries(Stream content);
    }

    /// <summary>
    /// Parses the entites returned by azure storage.
    /// </summary>
    public class DynamicEntityParser : IParser
    {
        private static readonly Logger Logger = LogManager.GetLogger("DynamicEntityParser");

        #region Namespaces
        private static readonly XNamespace AtomNs = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace DataServiceMetadataNs = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        #endregion

        /// <summary>
        /// Parses the entries in the XML Stream.
        /// </summary>
        /// <param name="content">The Xml content.</param>
        /// <returns>
        /// The entities.
        /// </returns>
        public IEnumerable<ExpandoEntity> ParseEntries(Stream content)
        {
            if (content == null) throw new ArgumentNullException("content");
            return XDocument.Load(content).Descendants(AtomNs + "entry").Select(ParseEntry);
        }

        /// <summary>
        /// Parses the entry and its values from the xml element.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>An <see cref="ExpandoEntity"/>.</returns>
        private static ExpandoEntity ParseEntry(XContainer entry)
        {
            var propertyNodes = entry.Descendants(DataServiceMetadataNs + "properties").DescendantNodes().Where(x => x is XElement);
            Logger.Log(LogLevel.Info, "Parsing Entry with {0} Values", propertyNodes.Count());
            var expandoEntity = new ExpandoEntity();
            foreach (XElement propertyNode in propertyNodes)
            {
                string localName = propertyNode.Name.LocalName;
                var value = GetValue(propertyNode);
                expandoEntity[localName] = value;
                Logger.Log(LogLevel.Debug, "{0} : {1} ({2}) -> {3}", localName, value, value != null ? value.GetType().Name : "Null", expandoEntity[localName]);
            }

            return expandoEntity;
        }

        /// <summary>
        /// Gets the value from the node using the null & type attributes.
        /// </summary>
        /// <param name="propertyNode">The property node.</param>
        /// <returns>The value of the node parsed as the correct type.</returns>
        private static object GetValue(XElement propertyNode)
        {
            if (propertyNode == null)   return null;
            var nullAttr = propertyNode.Attribute(DataServiceMetadataNs + "null");
            
            if (nullAttr != null && bool.Parse(nullAttr.Value)) return null;
            var typeAttr = propertyNode.Attribute(DataServiceMetadataNs + "type");

            string type = typeAttr != null ? typeAttr.Value : "string";
            return GetValueParser(type).Invoke(propertyNode.Value);
        }

        /// <summary>
        /// Gets the parser based on the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An instance of the parser</returns>
        private static ValueParser GetValueParser(string type)
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