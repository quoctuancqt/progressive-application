namespace AccentMSAddins.Services.Common
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlUtils
    {
        public static string Serialize(object value)
        {
            return Serialize(value, false);
        }

        public static string Serialize(object value, bool prettyFormatting = false, bool showXmlDeclaration = false, bool omitNamespaces = false, string rootName = null)
        {
            // Null uses default namespaces
            XmlSerializerNamespaces namespaces = null;

            if (omitNamespaces)
            {
                // Use empty namespace instead
                namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");
            }

            return Serialize(value, prettyFormatting, showXmlDeclaration, namespaces, rootName);
        }

        public static string Serialize(object value, bool prettyFormatting, bool showXmlDeclaration, XmlSerializerNamespaces namespaces, string rootName = null)
        {
            using (StringWriter writer = new StringWriter())
            {
                XmlWriterSettings xmlsettings = new XmlWriterSettings
                {
                    CheckCharacters = false,
                    OmitXmlDeclaration = !showXmlDeclaration
                };

                if (prettyFormatting)
                {
                    xmlsettings.Indent = true;
                    xmlsettings.NewLineOnAttributes = true;
                }

                XmlWriter xmlwriter = XmlWriter.Create(writer, xmlsettings);

                XmlRootAttribute root = null;
                if (!string.IsNullOrEmpty(rootName))
                    root = new XmlRootAttribute(rootName);

                XmlSerializer serializer = new XmlSerializer(value.GetType(), root);
                serializer.Serialize(xmlwriter, value, namespaces);

                string xml = writer.ToString();

                return RemoveInvalidCharacters(xml);
            }
        }

        public static T Deserialize<T>(string xml)
        {
            using (StringReader reader = new StringReader(xml))
            using (XmlReader xmlReader = XmlReader.Create(reader, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, CheckCharacters = false }))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(xmlReader);
            }
        }

        public static T Deserialize<T>(string xml, string overrideRootName)
        {
            using (StringReader reader = new StringReader(xml))
            {
                var attribs = new XmlAttributes();
                var overrides = new XmlAttributeOverrides();
                attribs.XmlRoot = new XmlRootAttribute(overrideRootName);
                overrides.Add(typeof(T), attribs);
                XmlSerializer serializer = new XmlSerializer(typeof(T), overrides);
                return (T)serializer.Deserialize(reader);
            }
        }

        public static object Deserialize(Type type, string xml)
        {
            using (StringReader reader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(type);
                return serializer.Deserialize(reader);
            }
        }

        public static object Deserialize(string xml)
        {
            return Deserialize(Assembly.GetCallingAssembly(), xml);
        }

        // This routine, and the one above it, are meant to deserialize classes when the calling code may not know
        // the exact type that is being deserialized, and there may not be an ancestor class to make things easier.
        // This routine is not expected to be used in high-volume situations, and that is not its current usage.
        // In informal benchmarks, calling this routine takes about 1.5 times as long as the version where the exact type
        // is known, and calling the one above it adds another 10% to that (but it's still not a lot of time--a fraction of a millisecond).
        public static object Deserialize(Assembly assemblyContainingType, string xml)
        {
            return Deserialize(assemblyContainingType, assemblyContainingType.GetName().Name, xml);
        }

        public static object Deserialize(Assembly assemblyContainingType, string classNamespace, string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            string typeName = classNamespace + "." + xmlDoc.FirstChild.Name;
            Type type = assemblyContainingType.GetType(typeName);

            return Deserialize(type, xml);
        }

        public static string XmlEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode node = xmlDoc.CreateElement("value");
            node.InnerText = value;
            return node.InnerXml;
        }

        public static string XmlDecode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode node = xmlDoc.CreateElement("value");
            node.InnerXml = value;
            return node.InnerText;
        }

        public static string RemoveInvalidCharacters(string xml)
        {
            char[] validChars = xml.Where(XmlConvert.IsXmlChar).ToArray();
            return new string(validChars);
        }
    }
}
