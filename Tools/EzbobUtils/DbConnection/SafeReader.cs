namespace Ezbob.Database {
	using System;
	using System.Data;
	using System.Data.Common;
	using Utils.ParsedValue;

	public class SafeReader {
		#region public

		#region constructor

		public SafeReader(DataRow oRow) {
			m_oRow = oRow;
			m_oReader = null;
		} // constructor

		public SafeReader(DbDataReader oReader) {
			m_oReader = oReader;
			m_oRow = null;
		} // constructor

		#endregion constructor

		#region indexer wrappers

		#region method Bool

		public bool Bool(string index) {
			return this[index, default(bool)];
		} // Bool

		#endregion method Bool

		#region method Int

		public int Int(string index) {
			return this[index, default(int)];
		} // Int

		#endregion method Int

		#region method IntWithDefault

		public int IntWithDefault(string index, int defaultValue) {
			return this[index, defaultValue];
		} // IntWithDefault

		#endregion method IntWithDefault

		#region method Decimal

		public decimal Decimal(string index) {
			return this[index, default(Decimal)];
		} // Decimal

		#endregion method Decimal

		#region method DateTime

		public DateTime DateTime(string index) {
			return this[index, default(DateTime)];
		} // DateTime

		#endregion method DateTime

		#region method String

		public string String(string index, string sDefault = null) {
			return this[index, sDefault];
		} // String

		#endregion method String

		#endregion indexer wrappers

		#region indexer

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

		#endregion indexer

		#endregion public

		#region private

		#region method ColumnOrDefault

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

		#endregion method ColumnOrDefault

		private readonly DataRow m_oRow;
		private readonly DbDataReader m_oReader;

		#endregion private
	} // class SafeReader
} // namespace
