namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Text;
	using Extensions;

	[DataContract]
	public class SortedTable<TRowKey, TColumnKey, TData> {
		public SortedTable() {
			this.data = new SortedDictionary<TRowKey, SortedDictionary<TColumnKey, TData>>();
			ColumnKeys = new SortedSet<TColumnKey>();
		} // constructor

		public bool Contains(TRowKey oRowKey) {
			return this.data.ContainsKey(oRowKey);
		} // Contains

		public bool Contains(TRowKey oRowKey, TColumnKey oColumnKey) {
			if (this.data.ContainsKey(oRowKey)) {
				SortedDictionary<TColumnKey, TData> oRow = this.data[oRowKey];

				return oRow.ContainsKey(oColumnKey);
			} // if

			return false;
		} // Contains

		public SortedDictionary<TColumnKey, TData> this[TRowKey oRowKey] {
			get { 
				if (this.data.ContainsKey(oRowKey))
					return this.data[oRowKey];

				throw new KeyNotFoundException("Cannot find row by requested key");
			} // get
		} // indexer

		public TData this[TRowKey oRowKey, TColumnKey oColumnKey] {
			get {
				if (this.data.ContainsKey(oRowKey)) {
					SortedDictionary<TColumnKey, TData> oRow = this.data[oRowKey];

					if (oRow.ContainsKey(oColumnKey))
						return oRow[oColumnKey];
				} // if

				throw new KeyNotFoundException("Cannot find element by requested keys");
			} // get
			set {
				SortedDictionary<TColumnKey, TData> oRow;

				if (this.data.ContainsKey(oRowKey))
					oRow = this.data[oRowKey];
				else {
					oRow = new SortedDictionary<TColumnKey, TData>();
					this.data[oRowKey] = oRow;
				} // if

				oRow[oColumnKey] = value;
				ColumnKeys.Add(oColumnKey);
			} // set
		} // indexer

		public void ForEach(Action<TRowKey, TColumnKey, TData> oAction) {
			if (ReferenceEquals(oAction, null))
				throw new ArgumentNullException("oAction", "Action not specified.");

			foreach (var pair in this.data)
				foreach (var raip in pair.Value)
					oAction(pair.Key, raip.Key, raip.Value);
		} // ForEach

		public void ForEachRow(Action<TRowKey, SortedDictionary<TColumnKey, TData>> oAction) {
			if (ReferenceEquals(oAction, null))
				throw new ArgumentNullException("oAction", "Action not specified.");

			foreach (var pair in this.data)
				oAction(pair.Key, pair.Value);
		} // ForEach

		public SortedSet<TRowKey> RowKeys {
			get { return new SortedSet<TRowKey>(this.data.Keys); }
		} // RowKeys

		[DataMember]
		public SortedSet<TColumnKey> ColumnKeys { get; private set; }

		public void Clear() {
			this.data.Clear();
			ColumnKeys.Clear();
		} // Clear

		public bool IsEmpty {
			get { return this.data.Count < 1; }
		} // IsEmpty

		public bool HasAlignedColumns() {
			foreach (KeyValuePair<TRowKey, SortedDictionary<TColumnKey, TData>> pair in this.data) {
				SortedDictionary<TColumnKey, TData> column = pair.Value;

				if (column.Count != ColumnKeys.Count)
					return false;

				foreach (KeyValuePair<TColumnKey, TData> pp in column)
					if (!ColumnKeys.Contains(pp.Key))
						return false;
			} // for each row

			return true;
		} // HasAlignedColumns

		public string ToFormattedString(
			string sTitle = null,
			char cColSeparator = '|',
			char cLineChar = '-',
			char cLineColSeparator = '+',
			Func<TRowKey, string> oRowKeyToString = null,
			Func<TColumnKey, string> oColumnKeyToString = null,
			Func<TData, string> oDataToString = null 
		) {
			var oFirstRow = new List<string>();
			var oOtherRows = new SortedDictionary<TRowKey, List<string>>();

			var oRowKeyStr = new SortedDictionary<TRowKey, string>();

			sTitle = (sTitle ?? string.Empty).Trim();

			string sColSeparator = " " + cColSeparator + " ";
			string sLineColSeparator = string.Format("{0}{1}{0}", cLineChar, cLineColSeparator);

			int nLength = 0;
			int nFirstLength = sTitle.Length;

			if (ReferenceEquals(oRowKeyToString, null))
				oRowKeyToString = x => x.ToString();

			if (ReferenceEquals(oColumnKeyToString, null))
				oColumnKeyToString = x => x.ToString();

			if (ReferenceEquals(oDataToString, null))
				oDataToString = x => x.ToString();

			foreach (var oColumnKey in ColumnKeys) {
				string sKey = oColumnKeyToString(oColumnKey);
				nLength = nLength.Max(sKey.Length);
				oFirstRow.Add(sKey);
			} // foreach key

			foreach (KeyValuePair<TRowKey, SortedDictionary<TColumnKey, TData>> oRowKeyValues in this.data) {
				TRowKey oRowKey = oRowKeyValues.Key;
				SortedDictionary<TColumnKey, TData> pcts = oRowKeyValues.Value;

				string s = oRowKeyToString(oRowKey);
				oRowKeyStr[oRowKey] = s;
				nFirstLength = nFirstLength.Max(s.Length);

				var oRowValues = new List<string>();

				foreach (var pair in pcts) {
					string sValue = oDataToString(pair.Value);
					oRowValues.Add(sValue);
					nLength = nLength.Max(sValue.Length);
				} // for each value

				oOtherRows[oRowKey] = oRowValues;
			} // for each row

			nFirstLength++;

			string sFirstColFormat = string.Format("{{0,{0}}}", nFirstLength);
			string sOtherColsFormat = string.Format("{{0,{0}}}", nLength);

			var oLine = new StringBuilder();
			oLine.Append(new string(cLineChar, nFirstLength));
			for (var i = 0; i < oFirstRow.Count; i++) {
				oLine.Append(sLineColSeparator);
				oLine.Append(new string(cLineChar, nLength));
			} // for
			oLine.Append(cLineChar);

			string sLine = Environment.NewLine + oLine + Environment.NewLine;

			var os = new StringBuilder();

			os.Append(
				string.Format(sFirstColFormat, sTitle) + sColSeparator +
				string.Join(sColSeparator, oFirstRow.Select(x => string.Format(sOtherColsFormat, x)))
			);

			foreach (var pair in oOtherRows) {
				os.Append(sLine);

				os.Append(
					string.Format(sFirstColFormat, oRowKeyStr[pair.Key]) + sColSeparator +
					string.Join(sColSeparator, pair.Value.Select(x => string.Format(sOtherColsFormat, x)))
				);
			} // foreach

			os.Append(Environment.NewLine);

			return os.ToString();
		} // ToFormattedString

		[DataMember]
		private readonly SortedDictionary<TRowKey, SortedDictionary<TColumnKey, TData>> data;
	} // class SortedTable
} // namespace Ezbob.Utils
