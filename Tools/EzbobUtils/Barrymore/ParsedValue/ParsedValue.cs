namespace Ezbob.Utils.ParsedValue {
	using System;
	using System.Collections.Generic;

	public class ParsedValue : IConvertible {

		static ParsedValue() {
			ms_oTypeConvertors = new SortedDictionary<string, Convertor>();

			ms_oTypeConvertors[typeof (bool).ToString()] = ParsedValue.ToBoolean;
			ms_oTypeConvertors[typeof (char).ToString()] = ParsedValue.ToChar;
			ms_oTypeConvertors[typeof (sbyte).ToString()] = ParsedValue.ToSByte;
			ms_oTypeConvertors[typeof (byte).ToString()] = ParsedValue.ToByte;
			ms_oTypeConvertors[typeof (short).ToString()] = ParsedValue.ToInt16;
			ms_oTypeConvertors[typeof (ushort).ToString()] = ParsedValue.ToUInt16;
			ms_oTypeConvertors[typeof (int).ToString()] = ParsedValue.ToInt32;
			ms_oTypeConvertors[typeof (uint).ToString()] = ParsedValue.ToUInt32;
			ms_oTypeConvertors[typeof (long).ToString()] = ParsedValue.ToInt64;
			ms_oTypeConvertors[typeof (ulong).ToString()] = ParsedValue.ToUInt64;
			ms_oTypeConvertors[typeof (float).ToString()] = ParsedValue.ToSingle;
			ms_oTypeConvertors[typeof (double).ToString()] = ParsedValue.ToDouble;
			ms_oTypeConvertors[typeof (decimal).ToString()] = ParsedValue.ToDecimal;
			ms_oTypeConvertors[typeof (DateTime).ToString()] = ParsedValue.ToDateTime;
			ms_oTypeConvertors[typeof (string).ToString()] = ParsedValue.ToString;
			ms_oTypeConvertors[typeof (Guid).ToString()] = ParsedValue.ToGuid; 

			ms_oTypeConvertors[typeof (bool?).ToString()] = ParsedValue.ToBooleanOrNull;
			ms_oTypeConvertors[typeof (char?).ToString()] = ParsedValue.ToCharOrNull;
			ms_oTypeConvertors[typeof (sbyte?).ToString()] = ParsedValue.ToSByteOrNull; 
			ms_oTypeConvertors[typeof (byte?).ToString()] = ParsedValue.ToByteOrNull; 
			ms_oTypeConvertors[typeof (short?).ToString()] = ParsedValue.ToInt16OrNull; 
			ms_oTypeConvertors[typeof (ushort?).ToString()] = ParsedValue.ToUInt16OrNull; 
			ms_oTypeConvertors[typeof (int?).ToString()] = ParsedValue.ToInt32OrNull; 
			ms_oTypeConvertors[typeof (uint?).ToString()] = ParsedValue.ToUInt32OrNull; 
			ms_oTypeConvertors[typeof (long?).ToString()] = ParsedValue.ToInt64OrNull; 
			ms_oTypeConvertors[typeof (ulong?).ToString()] = ParsedValue.ToUInt64OrNull; 
			ms_oTypeConvertors[typeof (float?).ToString()] = ParsedValue.ToSingleOrNull; 
			ms_oTypeConvertors[typeof (double?).ToString()] = ParsedValue.ToDoubleOrNull; 
			ms_oTypeConvertors[typeof (decimal?).ToString()] = ParsedValue.ToDecimalOrNull; 
			ms_oTypeConvertors[typeof (DateTime?).ToString()] = ParsedValue.ToDateTimeOrNull; 
			ms_oTypeConvertors[typeof (Guid?).ToString()] = ParsedValue.ToGuidOrNull; 

			ms_oTypeConvertors[typeof (byte[]).ToString()] = ParsedValue.ToByteArray; 
			ms_oTypeConvertors[typeof (string[]).ToString()] = ParsedValue.ToStringArray; 
		} // static constructor

		public ParsedValue(object oVal, object oDefault = null) {
			if (!ReferenceEquals(oVal, null) && (oVal.GetType() == typeof (ParsedValue))) {
				// ReSharper disable PossibleNullReferenceException
				var pv = oVal as ParsedValue;

				m_oValue = pv.m_oValue;
				m_oDefault = pv.m_oDefault;
				// ReSharper restore PossibleNullReferenceException
			}
			else {
				m_oValue = oVal;
				m_oDefault = oDefault;
			}
		} // constructor

		public object Raw { get { return m_oValue ?? m_oDefault; } } // Raw

		public TypeCode GetTypeCode() {
			return Type.GetTypeCode(m_oValue.GetType());
		} // GetTypeCode

		public static implicit operator bool(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(bool) : val.ToBoolean();
		} // operator bool

		private static void ToBoolean(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToBoolean(provider);
		} // ToBoolean

		public bool ToBoolean(IFormatProvider provider = null) {
			bool parsedValue;

			if (!ReferenceEquals(m_oValue, null) && bool.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !bool.TryParse(m_oDefault.ToString(), out parsedValue))
				return false;

			return parsedValue;
		} // ToBoolean

		public static implicit operator char(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(char) : val.ToChar();
		} // operator char

		private static void ToChar(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToChar(provider);
		} // ToChar

		public char ToChar(IFormatProvider provider = null) {
			char parsedValue;

			if (!ReferenceEquals(m_oValue, null) && char.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !char.TryParse(m_oDefault.ToString(), out parsedValue))
				return default(char);

			return parsedValue;
		} // ToChar

		public static implicit operator sbyte(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(sbyte) : val.ToSByte();
		} // operator sbyte

		private static void ToSByte(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToSByte(provider);
		} // ToSByte

		public sbyte ToSByte(IFormatProvider provider = null) {
			sbyte parsedValue;

			if (!ReferenceEquals(m_oValue, null) && sbyte.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !sbyte.TryParse(m_oDefault.ToString(), out parsedValue))
				return default(sbyte);

			return parsedValue;
		} // ToSByte

		public static implicit operator byte(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(byte) : val.ToByte();
		} // operator byte

		private static void ToByte(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToByte(provider);
		} // ToByte

		public byte ToByte(IFormatProvider provider = null) {
			byte parsedValue;

			if (!ReferenceEquals(m_oValue, null) && byte.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !byte.TryParse(m_oDefault.ToString(), out parsedValue))
				return default(byte);

			return parsedValue;
		} // ToByte

		public static implicit operator short(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(short) : val.ToInt16();
		} // operator short

		private static void ToInt16(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToInt16(provider);
		} // ToInt16

		public short ToInt16(IFormatProvider provider = null) {
			short parsedValue;

			if (!ReferenceEquals(m_oValue, null) && short.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !short.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToInt16

		public static implicit operator ushort(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(ushort) : val.ToUInt16();
		} // operator ushort

		private static void ToUInt16(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToUInt16(provider);
		} // ToUInt16

		public ushort ToUInt16(IFormatProvider provider = null) {
			ushort parsedValue;

			if (!ReferenceEquals(m_oValue, null) && ushort.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !ushort.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToUInt16

		public static implicit operator int(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(int) : val.ToInt32();
		} // operator int

		private static void ToInt32(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToInt32(provider);
		} // ToUIn32

		public int ToInt32(IFormatProvider provider = null) {
			int parsedValue;

			if (!ReferenceEquals(m_oValue, null) && int.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !int.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToInt32

		public static implicit operator uint(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(uint) : val.ToUInt32();
		} // operator uint

		private static void ToUInt32(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToUInt32(provider);
		} // ToUInt32

		public uint ToUInt32(IFormatProvider provider = null) {
			uint parsedValue;

			if (!ReferenceEquals(m_oValue, null) && uint.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !uint.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToUInt32

		public static implicit operator long(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(long) : val.ToInt64();
		} // operator long

		private static void ToInt64(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToInt64(provider);
		} // ToInt64

		public long ToInt64(IFormatProvider provider = null) {
			long parsedValue;

			if (!ReferenceEquals(m_oValue, null) && long.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !long.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToInt64

		public static implicit operator ulong(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(ulong) : val.ToUInt64();
		} // operator ulong

		private static void ToUInt64(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToUInt64(provider);
		} // ToUInt64

		public ulong ToUInt64(IFormatProvider provider = null) {
			ulong parsedValue;

			if (!ReferenceEquals(m_oValue, null) && ulong.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !ulong.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToUInt64

		public static implicit operator float(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(float) : val.ToSingle();
		} // operator float

		private static void ToSingle(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToSingle(provider);
		} // ToSingle

		public float ToSingle(IFormatProvider provider = null) {
			float parsedValue;

			if (!ReferenceEquals(m_oValue, null) && float.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !float.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToSingle

		public static implicit operator double(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(double) : val.ToDouble();
		} // operator double

		private static void ToDouble(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToDouble(provider);
		} // ToDouble

		public double ToDouble(IFormatProvider provider = null) {
			double parsedValue;

			if (!ReferenceEquals(m_oValue, null) && double.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !double.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToDouble

		public static implicit operator decimal(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(decimal) : val.ToDecimal();
		} // operator decimal

		private static void ToDecimal(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToDecimal(provider);
		} // ToDecimal

		public decimal ToDecimal(IFormatProvider provider = null) {
			decimal parsedValue;

			if (!ReferenceEquals(m_oValue, null) && decimal.TryParse(m_oValue.ToString(), out parsedValue))
				return parsedValue;

			if (ReferenceEquals(m_oDefault, null) || !decimal.TryParse(m_oDefault.ToString(), out parsedValue))
				return 0;

			return parsedValue;
		} // ToDecimal

		public static implicit operator DateTime(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(DateTime) : val.ToDateTime();
		} // operator DateTime

		private static void ToDateTime(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToDateTime(provider);
		} // ToDateTime

		public DateTime ToDateTime(IFormatProvider provider = null) {
			DateTime parsedValue;

			if (!ReferenceEquals(m_oValue, null)) {
				if (m_oValue is DateTime)
					return DateTime.SpecifyKind(((DateTime)m_oValue), DateTimeKind.Utc);

				if (DateTime.TryParse(m_oValue.ToString(), out parsedValue))
					return DateTime.SpecifyKind(parsedValue, DateTimeKind.Utc);
			} // if

			if (!ReferenceEquals(m_oDefault, null)) {
				if (m_oDefault is DateTime)
					return DateTime.SpecifyKind(((DateTime)m_oValue), DateTimeKind.Utc);

				if (DateTime.TryParse(m_oDefault.ToString(), out parsedValue))
					return DateTime.SpecifyKind(parsedValue, DateTimeKind.Utc);
			} // if

			return default(DateTime);
		} // ToDateTime

		public static implicit operator string(ParsedValue val) {
			return ReferenceEquals(val, null) ? default(string) : val.ToString();
		} // operator string

		private static void ToString(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToString(provider);
		} // ToString

		public string ToString(IFormatProvider provider = null) {
			if (!ReferenceEquals(m_oValue, null))
				return m_oValue.ToString();

			if (!ReferenceEquals(m_oDefault, null))
				return m_oDefault.ToString();

			return string.Empty;
		} // ToString

		public string ToNullString(IFormatProvider provider = null) {
			return ReferenceEquals(m_oValue, null) || m_oValue == DBNull.Value ? null : m_oValue.ToString();
		} // ToNullString

		public object ToType(Type conversionType, IFormatProvider provider = null) {
			if (ReferenceEquals(conversionType, null))
				throw new NotImplementedException("Cannot convert " + GetType() + " to undefined type.");

			string sTypeName = conversionType.ToString();

			Convertor oConvertor = ms_oTypeConvertors.ContainsKey(sTypeName) ? ms_oTypeConvertors[sTypeName] : null;

			if (oConvertor != null) {
				object v;
				oConvertor(this, out v, provider);
				return v;
			} // if

			throw new NotImplementedException("Cannot convert " + GetType() + " to " + sTypeName + ".");
		} // ToType

		public static implicit operator Guid(ParsedValue val) {
			return ReferenceEquals(val, null) ? Guid.Empty : val.ToGuid();
		} // operator Guid

		private static void ToGuid(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToGuid(provider);
		} // ToGuid

		public Guid ToGuid(IFormatProvider provider = null) {
			return (Guid)m_oValue;
		} // ToGuid

		public static implicit operator bool?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToBooleanOrNull();
		} // operator bool?

		private static void ToBooleanOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToBooleanOrNull(provider);
		} // ToBooleanOrNull

		public bool? ToBooleanOrNull(IFormatProvider provider = null) {
			return new NullableValue<bool>(m_oValue, m_oDefault, provider);
		} // ToBooleanOrNull

		public static implicit operator char?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToCharOrNull();
		} // operator char?

		private static void ToCharOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToCharOrNull(provider);
		} // ToCharOrNull

		public char? ToCharOrNull(IFormatProvider provider = null) {
			return new NullableValue<char>(m_oValue, m_oDefault, provider);
		} // ToCharOrNull

		public static implicit operator sbyte?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToSByteOrNull();
		} // operator sbyte?

		private static void ToSByteOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToSByteOrNull(provider);
		} // ToSByteOrNull

		public sbyte? ToSByteOrNull(IFormatProvider provider = null) {
			return new NullableValue<sbyte>(m_oValue, m_oDefault, provider);
		} // ToSByteOrNull

		public static implicit operator byte?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToByteOrNull();
		} // operator byte?

		private static void ToByteOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToByteOrNull(provider);
		} // ToByteOrNull

		public byte? ToByteOrNull(IFormatProvider provider = null) {
			return new NullableValue<byte>(m_oValue, m_oDefault, provider);
		} // ToByteOrNull

		public static implicit operator short?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToInt16OrNull();
		} // operator short?

		private static void ToInt16OrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToInt16OrNull(provider);
		} // ToInt16OrNull

		public short? ToInt16OrNull(IFormatProvider provider = null) {
			return new NullableValue<short>(m_oValue, m_oDefault, provider);
		} // ToInt16OrNull

		public static implicit operator ushort?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToUInt16OrNull();
		} // operator ushort?

		private static void ToUInt16OrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToUInt16OrNull(provider);
		} // ToUInt16OrNull

		public ushort? ToUInt16OrNull(IFormatProvider provider = null) {
			return new NullableValue<ushort>(m_oValue, m_oDefault, provider);
		} // ToUInt16OrNull

		public static implicit operator int?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToInt32OrNull();
		} // operator int?

		private static void ToInt32OrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToInt32OrNull(provider);
		} // ToInt32OrNull

		public int? ToInt32OrNull(IFormatProvider provider = null) {
			return new NullableValue<int>(m_oValue, m_oDefault, provider);
		} // ToInt32OrNull

		public static implicit operator uint?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToUInt32OrNull();
		} // operator uint?

		private static void ToUInt32OrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToUInt32OrNull(provider);
		} // ToUInt32OrNull

		public uint? ToUInt32OrNull(IFormatProvider provider = null) {
			return new NullableValue<uint>(m_oValue, m_oDefault, provider);
		} // ToUInt32OrNull

		public static implicit operator long?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToInt64OrNull();
		} // operator long?

		private static void ToInt64OrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToInt64OrNull(provider);
		} // ToInt64OrNull

		public long? ToInt64OrNull(IFormatProvider provider = null) {
			return new NullableValue<long>(m_oValue, m_oDefault, provider);
		} // ToInt64OrNull

		public static implicit operator ulong?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToUInt64OrNull();
		} // operator ulong?

		private static void ToUInt64OrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToUInt64OrNull(provider);
		} // ToUInt64OrNull

		public ulong? ToUInt64OrNull(IFormatProvider provider = null) {
			return new NullableValue<ulong>(m_oValue, m_oDefault, provider);
		} // ToUInt64OrNull

		public static implicit operator float?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToSingleOrNull();
		} // operator float?

		private static void ToSingleOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToSingleOrNull(provider);
		} // ToSingleOrNull

		public float? ToSingleOrNull(IFormatProvider provider = null) {
			return new NullableValue<float>(m_oValue, m_oDefault, provider);
		} // ToSingleOrNull

		public static implicit operator double?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToDoubleOrNull();
		} // operator double?

		private static void ToDoubleOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToDoubleOrNull(provider);
		} // ToDoubleOrNull

		public double? ToDoubleOrNull(IFormatProvider provider = null) {
			return new NullableValue<double>(m_oValue, m_oDefault, provider);
		} // ToDoubleOrNull

		public static implicit operator decimal?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToDecimalOrNull();
		} // operator decimal?

		private static void ToDecimalOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToDecimalOrNull(provider);
		} // ToDecimalOrNull

		public decimal? ToDecimalOrNull(IFormatProvider provider = null) {
			return new NullableValue<decimal>(m_oValue, m_oDefault, provider);
		} // ToDecimalOrNull

		public static implicit operator DateTime?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToDateTimeOrNull();
		} // operator DateTime?

		private static void ToDateTimeOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToDateTimeOrNull(provider);
		} // ToDateTimeOrNull

		public DateTime? ToDateTimeOrNull(IFormatProvider provider = null) {
			return new NullableValue<DateTime>(m_oValue, m_oDefault, provider);
		} // ToDateTimeOrNull

		public static implicit operator byte[](ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToByteArray();
		} // operator byte[]

		private static void ToByteArray(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToByteArray(provider);
		} // ToDecimalOrNull

		public byte[] ToByteArray(IFormatProvider provider = null) {
			return (byte[])m_oValue;
		} // ToByteArray

		public static implicit operator string[](ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToStringArray();
		} // operator string[]

		private static void ToStringArray(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToStringArray(provider);
		} // ToDecimalOrNull

		public string[] ToStringArray(IFormatProvider provider = null) {
			return (string[])m_oValue;
		} // ToStringArray

		public static implicit operator Guid?(ParsedValue val) {
			return ReferenceEquals(val, null) ? null : val.ToGuidOrNull();
		} // operator Guid?

		private static void ToGuidOrNull(ParsedValue pv, out object v, IFormatProvider provider = null) {
			v = pv.ToGuidOrNull(provider);
		} // ToGuidOrNull

		public Guid? ToGuidOrNull(IFormatProvider provider = null) {
			return new NullableValue<Guid>(m_oValue, m_oDefault, provider);
		} // ToGuidOrNull

		private readonly object m_oValue;
		private readonly object m_oDefault;

		private delegate void Convertor(ParsedValue a, out object b, IFormatProvider c);

		private static readonly SortedDictionary<string, Convertor> ms_oTypeConvertors;

		private class NullableValue<T> where T: struct {

			public static implicit operator T?(NullableValue<T> val) {
				return val.m_oParsedValue;
			} // to T?

			public NullableValue(object oValue, object oDefault, IFormatProvider provider = null) {
				m_oParsedValue = null;

				if (!ReferenceEquals(oValue, null)) {
					if (oValue == DBNull.Value)
						return;

					TryParse(oValue);
				} // if

				if (!ReferenceEquals(oDefault, null))
					TryParse(oDefault);
			} // constructor

			private void TryParse(object oValue) {
				string sValue = oValue.ToString();

				object oParsed = null;
				bool bSuccess = false;

				if (typeof(T) == typeof(bool)) {
					bool v;
					bSuccess = bool.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(char)) {
					char v;
					bSuccess = char.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(sbyte)) {
					sbyte v;
					bSuccess = sbyte.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(byte)) {
					byte v;
					bSuccess = byte.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(short)) {
					short v;
					bSuccess = short.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(ushort)) {
					ushort v;
					bSuccess = ushort.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(int)) {
					int v;
					bSuccess = int.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(uint)) {
					uint v;
					bSuccess = uint.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(long)) {
					long v;
					bSuccess = long.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(ulong)) {
					ulong v;
					bSuccess = ulong.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(float)) {
					float v;
					bSuccess = float.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(double)) {
					double v;
					bSuccess = double.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof(T) == typeof(decimal)) {
					decimal v;
					bSuccess = decimal.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				}
				else if (typeof (T) == typeof (DateTime)) {
					if (oValue is DateTime) {
						oParsed = oValue;
						bSuccess = true;
					}
					else {
						DateTime v;
						bSuccess = DateTime.TryParse(oValue.ToString(), out v);
						if (bSuccess)
							oParsed = v;
					}
				}
				else if (typeof(T) == typeof(Guid)) {
					Guid v;
					bSuccess = Guid.TryParse(sValue, out v);
					if (bSuccess)
						oParsed = v;
				} // if

				if (bSuccess)
					m_oParsedValue = (T?)oParsed;
			} // TryParse

			private T? m_oParsedValue;
		} // class NullableValue

	} // class ParsedValue
} // namespace Ezbob.Utils.ParsedValue
