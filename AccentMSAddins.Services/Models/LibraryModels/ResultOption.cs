using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AccentMSAddins.Services.Models
{
    public class ResultOption
    {
    }

    public class ExtraFields : ExtraFields<object>
    {
        public ExtraFields()
        {
        }

        public ExtraFields(IDictionary<string, object> other) : base(other)
        {
        }

        public new ExtraFields Clone()
        {
            ExtraFields fields = new ExtraFields();
            foreach (KeyValuePair<string, object> pair in this)
            {
                fields[pair.Key] = pair.Value;
            }
            return fields;
        }

        public bool IsModified(KeyValuePair<string, object> pair)
        {
            if (!ContainsKey(pair.Key))
                return false;
            // Compare strings
            object oldValue = this[pair.Key];
            object newValue = pair.Value;
            if (oldValue == null)
                return newValue != null;
            if (newValue == null)
                return false;
            return oldValue.ToString() != newValue.ToString();
        }
    }

    public class FieldNamingSchema
    {
        public string Element { get; set; }
        public string Name { get; set; }
    }

    public class ExtraFields<T> : Dictionary<string, T>, IXmlSerializable
    {
        protected FieldNamingSchema _schema = new FieldNamingSchema
        {
            Element = "Field",
            Name = "Name",
        };

        protected bool _ignoreNullValues = false;

        public ExtraFields() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ExtraFields(IDictionary<string, T> other) : base(other, StringComparer.OrdinalIgnoreCase)
        {
        }

        public bool IgnoreEmpty { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element)
            {
                object attrValue = reader.GetAttribute("IgnoreEmpty");
                if (attrValue != null)
                    IgnoreEmpty = bool.Parse(attrValue.ToString());

                if (reader.ReadToDescendant(_schema.Element))
                {
                    while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == _schema.Element)
                    {
                        string name = reader.GetAttribute(_schema.Name);
                        T value = (T)reader.ReadElementContentAs(typeof(T), null);
                        if (value == null && _ignoreNullValues)
                            continue;
                        if (!string.IsNullOrEmpty(name))
                            this[name] = value;
                    }
                }
                reader.Read();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (IgnoreEmpty)
                writer.WriteAttributeString("IgnoreEmpty", IgnoreEmpty.ToString());

            foreach (string name in Keys)
            {
                object value = this[name];
                if (value == null && _ignoreNullValues)
                    continue;
                writer.WriteStartElement(_schema.Element);
                writer.WriteAttributeString(_schema.Name, name);
                writer.WriteValue(value == null ? string.Empty : value.ToString());
                writer.WriteEndElement();
            }
        }

        // Get value or default if key is missing (instead of error)
        public object Get(string key)
        {
            T value;
            TryGetValue(key, out value);

            return value;
        }

        public ExtraFields<T> Clone()
        {
            ExtraFields<T> fields = new ExtraFields<T>()
            {
                _schema = _schema
            };
            foreach (KeyValuePair<string, T> pair in this)
            {
                fields[pair.Key] = pair.Value;
            }
            return fields;
        }
    }
}
