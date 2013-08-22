using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib
{
    public struct NullableInt : IXmlSerializable
    {
        private int value;
        private bool hasValue;

        private NullableInt(int value)
        {
            hasValue = true;
            this.value = value;
        }

        public bool HasValue
        {
            get { return hasValue; }
        }

        public int Value
        {
            get { return value; }
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.GetAttribute("nil") == "true")
            {
                ReadNullValue();
                return;
            }
            ReadNonNullValue(reader);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            throw new NotSupportedException();
        }

        private void ReadNullValue()
        {
            hasValue = false;
        }

        private void ReadNonNullValue(XmlReader reader)
        {
            reader.ReadStartElement();
            var s = reader.ReadString();

            if (string.IsNullOrEmpty(s)) return;

            value = Convert.ToInt32(s, CultureInfo.InvariantCulture);
            reader.ReadEndElement();
            hasValue = true;
        }

        public static implicit operator NullableInt(int value)
        {
            return new NullableInt(value);
        }

        public static implicit operator int?(NullableInt value)
        {
            if (value.HasValue)
            {
                return value.Value;
            }

            return null;
        }
    }
}