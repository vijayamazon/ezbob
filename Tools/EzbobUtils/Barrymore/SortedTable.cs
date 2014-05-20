﻿using System;
using System.Collections.Generic;

namespace Ezbob.Utils {
	using System.Linq;
	using System.Text;
	using Extensions;

	#region class SortedTable

	public class SortedTable<TRowKey, TColumnKey, TData> {
		#region public

		#region constructor

		public SortedTable() {
			m_oData = new SortedDictionary<TRowKey, SortedDictionary<TColumnKey, TData>>();
			ColumnKeys = new SortedSet<TColumnKey>();
		} // constructor

		#endregion constructor

		#region method Contains

		public bool Contains(TRowKey oRowKey) {
			return m_oData.ContainsKey(oRowKey);
		} // Contains

		public bool Contains(TRowKey oRowKey, TColumnKey oColumnKey) {
			if (m_oData.ContainsKey(oRowKey)) {
				SortedDictionary<TColumnKey, TData> oRow = m_oData[oRowKey];

				return oRow.ContainsKey(oColumnKey);
			} // if

			return false;
		} // Contains

		#endregion method Contains

		#region indexer

		public SortedDictionary<TColumnKey, TData> this[TRowKey oRowKey] {
			get { 
				if (m_oData.ContainsKey(oRowKey))
					return m_oData[oRowKey];

				throw new KeyNotFoundException("Cannot find row by requested key");
			} // get
		} // indexer

		public TData this[TRowKey oRowKey, TColumnKey oColumnKey] {
			get {
				if (m_oData.ContainsKey(oRowKey)) {
					SortedDictionary<TColumnKey, TData> oRow = m_oData[oRowKey];

					if (oRow.ContainsKey(oColumnKey))
						return oRow[oColumnKey];
				} // if

				throw new KeyNotFoundException("Cannot find element by requested keys");
			} // get
			set {
				SortedDictionary<TColumnKey, TData> oRow;

				if (m_oData.ContainsKey(oRowKey))
					oRow = m_oData[oRowKey];
				else {
					oRow = new SortedDictionary<TColumnKey, TData>();
					m_oData[oRowKey] = oRow;
				} // if

				oRow[oColumnKey] = value;
				ColumnKeys.Add(oColumnKey);
			} // set
		} // indexer

		#endregion indexer

		#region method ForEach

		public void ForEach(Action<TRowKey, TColumnKey, TData> oAction) {
			if (ReferenceEquals(oAction, null))
				throw new ArgumentNullException("oAction", "Action not specified.");

			foreach (var pair in m_oData)
				foreach (var raip in pair.Value)
					oAction(pair.Key, raip.Key, raip.Value);
		} // ForEach

		public void ForEachRow(Action<TRowKey, SortedDictionary<TColumnKey, TData>> oAction) {
			if (ReferenceEquals(oAction, null))
				throw new ArgumentNullException("oAction", "Action not specified.");

			foreach (var pair in m_oData)
				oAction(pair.Key, pair.Value);
		} // ForEach

		#endregion method ForEach

		#region property RowKeys

		public SortedSet<TRowKey> RowKeys {
			get { return new SortedSet<TRowKey>(m_oData.Keys); }
		} // RowKeys

		#endregion property RowKeys

		#region property ColumnKeys

		public SortedSet<TColumnKey> ColumnKeys { get; private set; }

		#endregion property ColumnKeys

		#region method Clear

		public void Clear() {
			m_oData.Clear();
		} // Clear

		#endregion method Clear

		#region property IsEmpty

		public bool IsEmpty {
			get { return m_oData.Count < 1; }
		} // IsEmpty

		#endregion property IsEmpty

		#region method ToFormattedString

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

			foreach (KeyValuePair<TRowKey, SortedDictionary<TColumnKey, TData>> oRowKeyValues in m_oData) {
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

			string sLine = Environment.NewLine + oLine.ToString() + Environment.NewLine;

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

		#endregion method ToFormattedString

		#endregion public

		#region private

		private readonly SortedDictionary<TRowKey, SortedDictionary<TColumnKey, TData>> m_oData;

		#endregion private
	} // class SortedTable

	#endregion class SortedTable
} // namespace Ezbob.Utils
