using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib
{
    public struct NullableDouble : IXmlSerializable
    {
        private double value;
        private bool hasValue;

        private NullableDouble(double value)
        {
            hasValue = true;
            this.value = value;
        }

        public bool HasValue
        {
            get { return hasValue; }
        }

        public double Value
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

            value = Convert.ToDouble(s, CultureInfo.InvariantCulture);
            reader.ReadEndElement();
            hasValue = true;
        }

        public static implicit operator NullableDouble(double value)
        {
            return new NullableDouble(value);
        }

        public static implicit operator double?(NullableDouble value)
        {
            if (value.HasValue)
            {
                return value.Value;
            }

            return null;
        }
    }
}