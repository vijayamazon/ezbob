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
// ReSharper disable EmptyGeneralCatchClause

		public TypeCode GetTypeCode() {
			return Type.GetTypeCode(m_oValue.GetType());
		} // GetTypeCode

		#region to boolean

		public static implicit operator bool(ParsedValue val) {
			return val.ToBoolean(); 
		} // operator bool

		public bool ToBoolean(IFormatProvider provider = null) {
			try { return Convert.ToBoolean(m_oValue, provider); } catch (Exception) {}
			return Convert.ToBoolean(m_oDefault, provider);
		} // ToBoolean

		#endregion to boolean

		#region to char

		public static implicit operator char(ParsedValue val) {
			return val.ToChar();
		} // operator char

		public char ToChar(IFormatProvider provider = null) {
			try { return Convert.ToChar(m_oValue, provider); } catch (Exception) {}
			return Convert.ToChar(m_oDefault, provider);
		} // ToChar

		#endregion to char

		#region to sbyte

		public static implicit operator sbyte(ParsedValue val) {
			return val.ToSByte();
		} // operator sbyte

		public sbyte ToSByte(IFormatProvider provider = null) {
			try { return Convert.ToSByte(m_oValue, provider); } catch (Exception) {}
			return Convert.ToSByte(m_oDefault, provider);
		} // ToSByte

		#endregion to sbyte

		#region to byte

		public static implicit operator byte(ParsedValue val) {
			return val.ToByte(); 
		} // operator byte

		public byte ToByte(IFormatProvider provider = null) {
			try { return Convert.ToByte(m_oValue, provider); } catch (Exception) {}
			return Convert.ToByte(m_oDefault, provider);
		} // ToByte

		#endregion to byte

		#region to short

		public static implicit operator short(ParsedValue val) {
			return val.ToInt16();
		} // operator short

		public short ToInt16(IFormatProvider provider = null) {
			try { return Convert.ToInt16(m_oValue, provider); } catch (Exception) {}
			return Convert.ToInt16(m_oDefault, provider);
		} // ToInt16

		#endregion to short

		#region to ushort

		public static implicit operator ushort(ParsedValue val) {
			return val.ToUInt16();
		} // operator ushort

		public ushort ToUInt16(IFormatProvider provider = null) {
			try { return Convert.ToUInt16(m_oValue, provider); } catch (Exception) {}
			return Convert.ToUInt16(m_oDefault, provider);
		} // ToUInt16

		#endregion to ushort

		#region to int

		public static implicit operator int(ParsedValue val) {
			return val.ToInt32();
		} // operator int

		public int ToInt32(IFormatProvider provider = null) {
			try { return Convert.ToInt32(m_oValue, provider); } catch (Exception) {}
			return Convert.ToInt32(m_oDefault, provider);
		} // ToInt32

		#endregion to int

		#region to uint

		public static implicit operator uint(ParsedValue val) {
			return val.ToUInt32();
		} // operator uint

		public uint ToUInt32(IFormatProvider provider = null) {
			try { return Convert.ToUInt32(m_oValue, provider); } catch (Exception) {}
			return Convert.ToUInt32(m_oDefault, provider);
		} // ToUInt32

		#endregion to uint

		#region to long

		public static implicit operator long(ParsedValue val) {
			return val.ToInt64();
		} // operator long

		public long ToInt64(IFormatProvider provider = null) {
			try { return Convert.ToInt64(m_oValue, provider); } catch (Exception) {}
			return Convert.ToInt64(m_oDefault, provider);
		} // ToInt64

		#endregion to long

		#region to ulong

		public static implicit operator ulong(ParsedValue val) {
			return val.ToUInt64();
		} // operator ulong

		public ulong ToUInt64(IFormatProvider provider = null) {
			try { return Convert.ToUInt64(m_oValue, provider); } catch (Exception) {}
			return Convert.ToUInt64(m_oDefault, provider);
		} // ToUInt64

		#endregion to ulong

		#region to float

		public static implicit operator float(ParsedValue val) {
			return val.ToSingle();
		} // operator float

		public float ToSingle(IFormatProvider provider = null) {
			try { return Convert.ToUInt64(m_oValue, provider); } catch (Exception) {}
			return Convert.ToUInt64(m_oDefault, provider);
		} // ToSingle

		#endregion to float

		#region to double

		public static implicit operator double(ParsedValue val) {
			return val.ToDouble();
		} // operator double

		public double ToDouble(IFormatProvider provider = null) {
			try { return Convert.ToDouble(m_oValue, provider); } catch (Exception) {}
			return Convert.ToDouble(m_oDefault, provider);
		} // ToDouble

		#endregion to double

		#region to decimal

		public static implicit operator decimal(ParsedValue val) {
			return val.ToDecimal();
		} // operator decimal

		public decimal ToDecimal(IFormatProvider provider = null) {
			try { return Convert.ToDecimal(m_oValue, provider); } catch (Exception) {}
			return Convert.ToDecimal(m_oDefault, provider);
		} // ToDecimal

		#endregion to decimal

		#region to datetime

		public static implicit operator DateTime(ParsedValue val) {
			return val.ToDateTime();
		} // operator DateTime

		public DateTime ToDateTime(IFormatProvider provider = null) {
			try { return Convert.ToDateTime(m_oValue, provider); } catch (Exception) {}
			return Convert.ToDateTime(m_oDefault, provider);
		} // ToDateTime

		#endregion to datetime

		#region to string

		public static implicit operator string(ParsedValue val) {
			return val.ToString();
		} // operator string

		public string ToString(IFormatProvider provider = null) {
			try { return Convert.ToString(m_oValue, provider); } catch (Exception) {}
			return Convert.ToString(m_oDefault, provider);
		} // ToString

		#endregion to string

		public object ToType(Type conversionType, IFormatProvider provider = null) {
			throw new NotImplementedException("Cannot convert " + this.GetType() + " to " + conversionType);
		} // ToType

// ReSharper restore EmptyGeneralCatchClause
		#endregion interface IConvertible and type cast operators

		#endregion public

		#region private

		private readonly object m_oValue;
		private readonly object m_oDefault;

		#endregion private
	} // class ParsedValue

	#endregion class ParsedValue
} // namespace Ezbob.Utils.ParsedValue
