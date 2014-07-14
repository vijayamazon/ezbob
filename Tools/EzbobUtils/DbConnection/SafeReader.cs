namespace Ezbob.Database {
	using System;
	using System.Data;
	using System.Data.Common;
	using System.Reflection;
	using Ezbob.Utils;
	using Ezbob.Utils.ParsedValue;

	#region class FieldNameAttribute

	[System.AttributeUsage(
		System.AttributeTargets.Property,
		AllowMultiple = false
	)]
	public class FieldNameAttribute : Attribute {
		#region constructor

		public FieldNameAttribute(string sFieldName) {
			Name = sFieldName;
		} // constructor

		#endregion constructor

		#region property Name

		public string Name {
			get { return m_sName; } // get
			private set { m_sName = string.IsNullOrWhiteSpace(value) ? string.Empty : value; } // set
		} // Name

		private string m_sName;

		#endregion property Name
	} // FieldNameAttribute

	#endregion class FieldNameAttribute

	#region class SafeReader

	public class SafeReader {
		#region public

		#region class SafeReaderFluentInterface

		public class SafeReaderFluentInterface {
			#region public

			#region constructor

			public SafeReaderFluentInterface(SafeReader oReader) {
				m_oReader = oReader;
				Reset();
			} // constructor

			#endregion constructor

			#region method Reset

			public SafeReaderFluentInterface Reset() {
				m_nIdx = 0;
				return this;
			} // Reset

			#endregion method Reset

			#region method To

			public SafeReaderFluentInterface To<T>(out T v) {
				v = (T)m_oReader[m_nIdx].ToType(typeof(T));
				m_nIdx++;
				return this;
			} // To

			#endregion method To

			#endregion public

			#region private

			private readonly SafeReader m_oReader;
			private int m_nIdx;

			#endregion private
		} // SafeReaderFluentInterface

		#endregion class SafeReaderFluentInterface

		#region constructor

		public SafeReader(DataRow oRow) : this(oRow, null) {
		} // constructor

		public SafeReader(DbDataReader oReader) : this(null, oReader) {
		} // constructor

		#endregion constructor

		#region property Read

		public SafeReaderFluentInterface Read {
			get { return m_oFluent.Reset(); }
		} // Read

		#endregion property Read

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

		#region method ContainsField

		public bool ContainsField(string sIdx) {
			if (!ReferenceEquals(m_oRow, null))
				return m_oRow.Table.Columns.Contains(sIdx);

			if (!ReferenceEquals(m_oReader, null)) {
				for (var i = 0; i < m_oReader.FieldCount; i++)
					if (m_oReader.GetName(i) == sIdx)
						return true;
			} // if

			return false;
		} // ContainsField

		public bool ContainsField(string sIdx, string sNotFoundIndicator) {
			if (!ReferenceEquals(m_oRow, null))
				return m_oRow.Table.Columns.Contains(sIdx);

			if (!ReferenceEquals(m_oReader, null)) {
				for (var i = 0; i < m_oReader.FieldCount; i++) {
					string sName = m_oReader.GetName(i);

					if (sName == sIdx)
						return true;

					if (sName == sNotFoundIndicator)
						return false;
				} // for
			} // if

			return false;
		} // ContainsField

		public bool ContainsField(int nIdx) {
			if (!ReferenceEquals(m_oRow, null))
				return ((0 <= nIdx) && (nIdx < m_oRow.Table.Columns.Count));

			if (!ReferenceEquals(m_oReader, null))
				return ((0 <= nIdx) && (nIdx < m_oReader.FieldCount));

			return false;
		} // ContainsField

		#endregion method ContainsField

		#region method Fill

		public T Fill<T>() where T: new() {
			var oInstance = new T();
			Fill(oInstance);
			return oInstance;
		} // Fill

		public void Fill(object oInstance) {
			oInstance.Traverse(FillProperty);
		} // Fill

		#endregion method Fill

		#region property Count

		public int Count {
			get {
				if (!ReferenceEquals(m_oRow, null))
					return m_oRow.Table.Columns.Count;

				if (!ReferenceEquals(m_oReader, null))
					return m_oReader.FieldCount;

				throw new NullReferenceException("Neither row nor DB reader specified.");
			} // get
		} // Count

		#endregion property Count

		#endregion public

		#region private

		#region constructor

		private SafeReader(DataRow oRow, DbDataReader oReader) {
			m_oReader = oReader;
			m_oRow = oRow;
			m_oFluent = new SafeReaderFluentInterface(this);
		} // constructor

		#endregion constructor

		#region method FillProperty

		private void FillProperty(object oInstance, PropertyInfo oPropertyInfo) {
			string sFieldName = oPropertyInfo.Name;

			object[] aryAttrList = oPropertyInfo.GetCustomAttributes(typeof (FieldNameAttribute), false);

			if (aryAttrList.Length > 0) {
				var fna = (FieldNameAttribute)aryAttrList[0];

				if (!string.IsNullOrWhiteSpace(fna.Name))
					sFieldName = fna.Name;
			} // if

			oPropertyInfo.SetValue(oInstance, this[sFieldName].ToType(oPropertyInfo.PropertyType));
		} // FillProperty

		#endregion method FillProperty

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

		#region properties

		private readonly DataRow m_oRow;
		private readonly DbDataReader m_oReader;
		private readonly SafeReaderFluentInterface m_oFluent;

		#endregion properties

		#endregion private
	} // class SafeReader

	#endregion class SafeReader
} // namespace
