namespace Ezbob.Utils.ParsedValue {
	using System;

	#region class ParsedValue

	public class ParsedValue : IConvertible {
		#region public

		#region constructor

		public ParsedValue(object oVal, object oDefault = null) {
			m_oValue = oVal;
			m_oDefault = oDefault;
		} // constructor

		#endregion constructor

		#region interface IConvertible and type cast operators

		public TypeCode GetTypeCode() {
			return Type.GetTypeCode(m_oValue.GetType());
		} // GetTypeCode

		#region to boolean

		public static implicit operator bool(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(bool) : val.ToBoolean();
		} // operator bool

		public bool ToBoolean(IFormatProvider provider = null) {
			bool parsedValue;

			if (!ReferenceEquals(m_oValue, null) && bool.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !bool.TryParse(m_oDefault.ToString(), out parsedValue))
				return false;

			return parsedValue;
		} // ToBoolean

		#endregion to boolean

		#region to char

		public static implicit operator char(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(char) : val.ToChar();
		} // operator char

		public char ToChar(IFormatProvider provider = null) {
			char parsedValue;

			if (!ReferenceEquals(m_oValue, null) && char.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !char.TryParse(m_oDefault.ToString(), out parsedValue))
				return default(char);

			return parsedValue;
		} // ToChar

		#endregion to char

		#region to sbyte

		public static implicit operator sbyte(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(sbyte) : val.ToSByte();
		} // operator sbyte

		public sbyte ToSByte(IFormatProvider provider = null) {
			sbyte parsedValue;

			if (!ReferenceEquals(m_oValue, null) && sbyte.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !sbyte.TryParse(m_oDefault.ToString(), out parsedValue))
				return default(sbyte);

			return parsedValue;
		} // ToSByte

		#endregion to sbyte

		#region to byte

		public static implicit operator byte(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(byte) : val.ToByte();
		} // operator byte

		public byte ToByte(IFormatProvider provider = null) {
			byte parsedValue;

			if (!ReferenceEquals(m_oValue, null) && byte.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !byte.TryParse(m_oDefault.ToString(), out parsedValue))
				return default(byte);

			return parsedValue;
		} // ToByte

		#endregion to byte

		#region to short

		public static implicit operator short(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(short) : val.ToInt16();
		} // operator short

		public short ToInt16(IFormatProvider provider = null) {
			short parsedValue;

			if (!ReferenceEquals(m_oValue, null) && short.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !short.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToInt16

		#endregion to short

		#region to ushort

		public static implicit operator ushort(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(ushort) : val.ToUInt16();
		} // operator ushort

		public ushort ToUInt16(IFormatProvider provider = null) {
			ushort parsedValue;

			if (!ReferenceEquals(m_oValue, null) && ushort.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !ushort.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToUInt16

		#endregion to ushort

		#region to int

		public static implicit operator int(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(int) : val.ToInt32();
		} // operator int

		public int ToInt32(IFormatProvider provider = null) {
			int parsedValue;

			if (!ReferenceEquals(m_oValue, null) && int.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !int.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToInt32

		#endregion to int

		#region to uint

		public static implicit operator uint(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(uint) : val.ToUInt32();
		} // operator uint

		public uint ToUInt32(IFormatProvider provider = null) {
			uint parsedValue;

			if (!ReferenceEquals(m_oValue, null) && uint.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !uint.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToUInt32

		#endregion to uint

		#region to long

		public static implicit operator long(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(long) : val.ToInt64();
		} // operator long

		public long ToInt64(IFormatProvider provider = null) {
			long parsedValue;

			if (!ReferenceEquals(m_oValue, null) && long.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !long.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToInt64

		#endregion to long

		#region to ulong

		public static implicit operator ulong(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(ulong) : val.ToUInt64();
		} // operator ulong

		public ulong ToUInt64(IFormatProvider provider = null) {
			ulong parsedValue;

			if (!ReferenceEquals(m_oValue, null) && ulong.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !ulong.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToUInt64

		#endregion to ulong

		#region to float

		public static implicit operator float(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(float) : val.ToSingle();
		} // operator float

		public float ToSingle(IFormatProvider provider = null) {
			float parsedValue;

			if (!ReferenceEquals(m_oValue, null) && float.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !float.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToSingle

		#endregion to float

		#region to double

		public static implicit operator double(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(double) : val.ToDouble();
		} // operator double

		public double ToDouble(IFormatProvider provider = null) {
			double parsedValue;

			if (!ReferenceEquals(m_oValue, null) && double.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !double.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToDouble

		#endregion to double

		#region to decimal

		public static implicit operator decimal(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(decimal) : val.ToDecimal();
		} // operator decimal

		public decimal ToDecimal(IFormatProvider provider = null) {
			decimal parsedValue;

			if (!ReferenceEquals(m_oValue, null) && decimal.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !decimal.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToDecimal

		#endregion to decimal

		#region to datetime

		public static implicit operator DateTime(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(DateTime) : val.ToDateTime();
		} // operator DateTime

		public DateTime ToDateTime(IFormatProvider provider = null) {
			DateTime parsedValue;

			if (!ReferenceEquals(m_oValue, null))
			{
				if (m_oValue is DateTime)
				{
					return (DateTime)m_oValue;
				}

				if (DateTime.TryParse(m_oValue.ToString(), out parsedValue))
				{
					return parsedValue;
				}
			}

			if (!ReferenceEquals(m_oDefault, null))
			{
				if (m_oDefault is DateTime)
				{
					return (DateTime)m_oDefault;
				}

				if (DateTime.TryParse(m_oDefault.ToString(), out parsedValue))
				{
					return parsedValue;
				}
			}

			return default(DateTime);
		} // ToDateTime

		#endregion to datetime

		#region to string

		public static implicit operator string(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(string) : val.ToString();
		} // operator string

		public string ToString(IFormatProvider provider = null) {
			if (!ReferenceEquals(m_oValue, null))
				return m_oValue.ToString();

			if (!ReferenceEquals(m_oDefault, null))
				return m_oDefault.ToString();

			return string.Empty;
		} // ToString

		#endregion to string

		public object ToType(Type conversionType, IFormatProvider provider = null) {
			throw new NotImplementedException("Cannot convert " + GetType() + " to " + conversionType);
		} // ToType

		#endregion interface IConvertible and type cast operators

		#endregion public

		#region private

		private readonly object m_oValue;
		private readonly object m_oDefault;

		#endregion private
	} // class ParsedValue

	#endregion class ParsedValue
} // namespace Ezbob.Utils.ParsedValue
