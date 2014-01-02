namespace Ezbob.Database {
	using System;
	using System.Data;
	using System.Data.Common;
	using Utils;
	using Utils.ParsedValue;

	public class SafeReader {
		private readonly DataRow m_oRow;
		private readonly DbDataReader m_oReader;
		private readonly SafeParser safeParser = new SafeParser();

		public SafeReader(DataRow oRow) {
			m_oRow = oRow;
			m_oReader = null;
		} // constructor

		public SafeReader(DbDataReader oReader) {
			m_oReader = oReader;
			m_oRow = null;
		} // constructor

		public ParsedValue this[string index, object oDefault = null] {
			get {
				return new ParsedValue(ColumnOrDefault(index, oDefault), oDefault);
			} // get
		} // indexer

		public ParsedValue this[int index, object oDefault = null] {
			get {
				return new ParsedValue(ColumnOrDefault(index, oDefault), oDefault);
			} // get
		} // indexer

		private object ColumnOrDefault(string sIdx, object oDefault) {
			if (!ReferenceEquals(m_oRow, null))
				return m_oRow.Table.Columns.Contains(sIdx) ? m_oRow[sIdx] : oDefault;

			if (!ReferenceEquals(m_oReader, null)) {
				try {
					return m_oReader[sIdx];
				}
				catch (Exception) {
					return oDefault;
				} // try
			} // try

			throw new NullReferenceException("Neither row nor DB reader specified.");
		} // ColumnOrDefault

		private object ColumnOrDefault(int nIdx, object oDefault) {
			if (!ReferenceEquals(m_oRow, null))
				return ((0 <= nIdx) && (nIdx < m_oRow.Table.Columns.Count)) ? m_oRow[nIdx] : oDefault;

			if (!ReferenceEquals(m_oReader, null)) {
				try {
					return m_oReader[nIdx];
				}
				catch (Exception) {
					return oDefault;
				} // try
			} // try

			throw new NullReferenceException("Neither row nor DB reader specified.");
		} // ColumnOrDefault

		public int Int(string index, int defaultValue)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
				return safeParser.GetInt(m_oRow[index], defaultValue);
			}

			return defaultValue;
		}

		public int Int(string index)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
				return safeParser.GetInt(m_oRow[index]);
			}

			return default(int);
		}

		public decimal Decimal(string index, decimal defaultValue)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
				return safeParser.GetDecimal(m_oRow[index], defaultValue);
			}

			return defaultValue;
		}

		public decimal Decimal(string index)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
				return safeParser.GetDecimal(m_oRow[index]);
			}

			return default(decimal);
		}

		public bool Bool(string index, bool defaultValue)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
			return safeParser.GetBool(m_oRow[index], defaultValue);
			}

			return defaultValue;
		}

		public bool Bool(string index)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
			return safeParser.GetBool(m_oRow[index]);
			}

			return default(bool);
		}

		public DateTime DateTime(string index, DateTime defaultValue)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
			return safeParser.GetDateTime(m_oRow[index], defaultValue);
			}

			return defaultValue;
		}

		public DateTime DateTime(string index)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
				return safeParser.GetDateTime(m_oRow[index]);
			}

			return default(DateTime);
		}

		public string String(string index, string defaultValue)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
				return m_oRow[index].ToString();
			}

			return defaultValue;
		}

		public string String(string index)
		{
			if (m_oRow.Table.Columns.Contains(index))
			{
				return m_oRow[index].ToString();
			}

			return default(string);
		}
	}
}
