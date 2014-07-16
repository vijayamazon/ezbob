namespace Ezbob.Utils.ParsedValue {
	using System.Collections.Generic;
	using System.Globalization;

	public class ParsedValueCache {
		#region public

		#region constructor

		public ParsedValueCache() {
			m_oData = new List<ParsedValue>();
			m_oMap = new SortedDictionary<string, ParsedValue>();
		} // constructor

		#endregion constructor

		#region method Add

		public void Add(string sKey, ParsedValue pv) {
			if (pv == null)
				return;

			m_oData.Add(pv);

			if (string.IsNullOrWhiteSpace(sKey))
				sKey = "Column" + m_oData.Count.ToString(CultureInfo.InvariantCulture);

			m_oMap[sKey] = pv;
		} // Add

		#endregion method Add

		#region property Count

		public int Count {
			get { return m_oData.Count; }
		} // Count

		#endregion property Count

		#region method Contains

		public bool Contains(int nIdx) {
			return (0 <= nIdx) && (nIdx < m_oData.Count);
		} // Contains

		public bool Contains(string sIdx) {
			return m_oMap.ContainsKey(sIdx);
		} // Contains

		#endregion method Contains

		#region indexer

		public ParsedValue this[int nIdx, object oDefault = null] {
			get { return Contains(nIdx) ? m_oData[nIdx] : new ParsedValue(null, oDefault); }
		} // indexer

		public ParsedValue this[string sIdx, object oDefault = null] {
			get { return Contains(sIdx) ? m_oMap[sIdx] : new ParsedValue(null, oDefault); }
		} // indexer

		#endregion indexer

		#endregion public

		#region private

		private readonly List<ParsedValue> m_oData;
		private readonly SortedDictionary<string, ParsedValue> m_oMap;

		#endregion private
	} // class ParsedValueCache
} // namespace
