using System.Collections.Generic;

namespace EzAnalyticsConsoleClient {
	#region class GoogleDataItem

	class GoogleDataItem {
		#region public

		#region constructor

		public GoogleDataItem() {
			m_oDimensions = new SortedDictionary<GoogleReportDimensions, string>();
			m_oMetrics = new SortedDictionary<GoogleReportMetrics, int>();
		} // constructor

		#endregion constructor

		#region indexer

		public string this[GoogleReportDimensions dim] {
			get { return m_oDimensions.ContainsKey(dim) ? m_oDimensions[dim] : null; }
			set { m_oDimensions[dim] = value; }
		} // dimensions indexer

		public int this[GoogleReportMetrics m] {
			get { return m_oMetrics.ContainsKey(m) ? m_oMetrics[m] : 0; }
			set { m_oMetrics[m] = value; }
		} // metrics indexer

		#endregion indexer

		#endregion public

		#region private

		private readonly SortedDictionary<GoogleReportDimensions, string> m_oDimensions;
		private readonly SortedDictionary<GoogleReportMetrics, int> m_oMetrics;

		#endregion private
	} // class GoogleDataItem

	#endregion class GoogleDataItem
} // namespace EzAnalyticsConsoleClient
