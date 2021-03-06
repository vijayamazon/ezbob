﻿namespace Ezbob.Utils.ParsedValue {
	using System.Collections.Generic;
	using System.Globalization;

	public class ParsedValueCache {

		public ParsedValueCache() {
			m_oData = new List<ParsedValue>();
			m_oMap = new SortedDictionary<string, ParsedValue>();
			m_oNames = new List<string>();
		} // constructor

		public void Add(string sKey, ParsedValue pv) {
			if (pv == null)
				return;

			if (string.IsNullOrWhiteSpace(sKey))
				sKey = "Column" + m_oData.Count.ToString(CultureInfo.InvariantCulture);

			m_oData.Add(pv);
			m_oNames.Add(sKey);
			m_oMap[sKey] = pv;
		} // Add

		public int Count {
			get { return m_oData.Count; }
		} // Count

		public bool Contains(int nIdx) {
			return (0 <= nIdx) && (nIdx < m_oData.Count);
		} // Contains

		public bool Contains(string sIdx) {
			return m_oMap.ContainsKey(sIdx);
		} // Contains

		public string GetName(int nIdx) {
			return Contains(nIdx) ? m_oNames[nIdx] : null;
		} // GetName

		public ParsedValue this[int nIdx, object oDefault = null] {
			get { return Contains(nIdx) ? m_oData[nIdx] : new ParsedValue(null, oDefault); }
		} // indexer

		public ParsedValue this[string sIdx, object oDefault = null] {
			get { return Contains(sIdx) ? m_oMap[sIdx] : new ParsedValue(null, oDefault); }
		} // indexer

		private readonly List<ParsedValue> m_oData;
		private readonly List<string> m_oNames;
		private readonly SortedDictionary<string, ParsedValue> m_oMap;

	} // class ParsedValueCache
} // namespace
