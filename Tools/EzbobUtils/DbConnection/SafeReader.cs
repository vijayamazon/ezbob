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

		#region method CreateEmpty

		public static SafeReader CreateEmpty() {
			return new SafeReader(null, null, true, false);
		} // CreateEmpty

		#endregion method CreateEmpty

		#region constructor

		public SafeReader(DataRow oRow) : this(oRow, null, false, false) {
		} // constructor

		public SafeReader(DbDataReader oReader) : this(null, oReader, false, false) {
		} // constructor

		#endregion constructor

		#region property Read

		public SafeReaderFluentInterface Read {
			get { return m_oFluent.Reset(); }
		} // Read

		#endregion property Read

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

			if (!ReferenceEquals(m_oCache, null))
				return m_oCache.Contains(sIdx);

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

			if (!ReferenceEquals(m_oCache, null))
				return m_oCache.Contains(sIdx);

			return false;
		} // ContainsField

		public bool ContainsField(int nIdx) {
			if (!ReferenceEquals(m_oRow, null))
				return ((0 <= nIdx) && (nIdx < m_oRow.Table.Columns.Count));

			if (!ReferenceEquals(m_oReader, null))
				return ((0 <= nIdx) && (nIdx < m_oReader.FieldCount));

			if (!ReferenceEquals(m_oCache, null))
				return m_oCache.Contains(nIdx);

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

				if (!ReferenceEquals(m_oCache, null))
					return m_oCache.Count;

				if (m_bAllowNoSource)
					return 0;

				throw new NullReferenceException("Neither row nor DB reader specified.");
			} // get
		} // Count

		#endregion property Count

		#region method ToCache

		public SafeReader ToCache() {
			var sr = new SafeReader(null, null, false, true);

			if (ReferenceEquals(m_oRow, null) && ReferenceEquals(m_oReader, null))
				return sr;

			for (int i = 0; i < Count; i++) {
				string sName = !ReferenceEquals(m_oRow, null)
					? m_oRow.Table.Columns[i].ColumnName
					: m_oReader.GetName(i);

				sr.m_oCache.Add(sName, this[i]);
			} // for

			return sr;
		} // ToCache

		#endregion method ToCache

		#region property IsEmpty

		public bool IsEmpty {
			get { return
				ReferenceEquals(m_oReader, null) &&
				ReferenceEquals(m_oRow, null) &&
				ReferenceEquals(m_oCache, null) &&
				m_bAllowNoSource;
			}
		} // IsEmpty

		#endregion property IsEmpty

		#endregion public

		#region private

		#region constructor

		private SafeReader(DataRow oRow, DbDataReader oReader, bool bAllowNoSource, bool bAllowCache) {
			m_oReader = oReader;
			m_oRow = oRow;
			m_oFluent = new SafeReaderFluentInterface(this);
			m_bAllowNoSource = bAllowNoSource;
			m_oCache = bAllowCache ? new ParsedValueCache() : null;
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

			if (!ReferenceEquals(m_oCache, null))
				return m_oCache[sIdx, oDefault];

			if (m_bAllowNoSource)
				return oDefault;

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

			if (!ReferenceEquals(m_oCache, null))
				return m_oCache[nIdx, oDefault];

			if (m_bAllowNoSource)
				return oDefault;

			throw new NullReferenceException("Neither row nor DB reader specified.");
		} // ColumnOrDefault

		#endregion method ColumnOrDefault

		#region properties

		private readonly DataRow m_oRow;
		private readonly DbDataReader m_oReader;
		private readonly SafeReaderFluentInterface m_oFluent;
		private readonly bool m_bAllowNoSource;
		private readonly ParsedValueCache m_oCache;

		#endregion properties

		#endregion private
	} // class SafeReader

	#endregion class SafeReader
} // namespace
