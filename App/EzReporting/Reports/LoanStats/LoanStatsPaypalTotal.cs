namespace Reports.LoanStats {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class LoanStatsPaypalTotal {
		public LoanStatsPaypalTotal(int nMarketplaceID, DateTime oUpdated, decimal nTotal) {
			m_oData = new SortedDictionary<int, SortedDictionary<DateTime, decimal>>();

			Add(nMarketplaceID, oUpdated, nTotal);
		} // constructor

		public void Add(int nMarketplaceID, DateTime oUpdated, decimal nTotal) {
			if (!m_oData.ContainsKey(nMarketplaceID))
				m_oData[nMarketplaceID] = new SortedDictionary<DateTime, decimal>();

			m_oData[nMarketplaceID][oUpdated] = nTotal;
		} // Add

		public decimal Calculate(DateTime oEdge) {
			var oResult = new SortedDictionary<int, decimal>();

			foreach (KeyValuePair<int, SortedDictionary<DateTime, decimal>> pair in m_oData) {
				foreach (KeyValuePair<DateTime, decimal> oTotalPair in pair.Value) {
					if (oTotalPair.Key <= oEdge)
						oResult[pair.Key] = oTotalPair.Value;
					else
						break;
				} // for each
			} // for each

			return oResult.Count > 0 ? oResult.Sum(pair => pair.Value) : 0;
		} // Calculate

		private SortedDictionary<int, SortedDictionary<DateTime, decimal>> m_oData;
	} // class LoanStatsPaypalTotal
} // namespace
