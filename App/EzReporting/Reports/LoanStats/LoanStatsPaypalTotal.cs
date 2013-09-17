using System;
using System.Collections.Generic;
using System.Linq;

namespace Reports {
	internal class LoanStatsPaypalTotal {
		#region public

		#region constructor

		public LoanStatsPaypalTotal(int nMarketplaceID, DateTime oUpdated, decimal nTotal) {
			m_oData = new SortedDictionary<int, SortedDictionary<DateTime, decimal>>();

			Add(nMarketplaceID, oUpdated, nTotal);
		} // constructor

		#endregion constructor

		#region method Add

		public void Add(int nMarketplaceID, DateTime oUpdated, decimal nTotal) {
			if (!m_oData.ContainsKey(nMarketplaceID))
				m_oData[nMarketplaceID] = new SortedDictionary<DateTime, decimal>();

			m_oData[nMarketplaceID][oUpdated] = nTotal;
		} // Add

		#endregion method Add

		#region method Calculate

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

		#endregion method Calculate

		#endregion public

		#region private

		private SortedDictionary<int, SortedDictionary<DateTime, decimal>> m_oData;

		#endregion private
	} // class LoanStatsPaypalTotal
} // namespace
