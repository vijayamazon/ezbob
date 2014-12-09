using System.Collections.Generic;

namespace GoogleAnalyticsLib
{

	class GoogleDataItem {

		public GoogleDataItem() {
			m_oDimensions = new SortedDictionary<GoogleReportDimensions, string>();
			m_oMetrics = new SortedDictionary<GoogleReportMetrics, int>();
		} // constructor

		public string this[GoogleReportDimensions dim] {
			get { return m_oDimensions.ContainsKey(dim) ? m_oDimensions[dim] : null; }
			set { m_oDimensions[dim] = value; }
		} // dimensions indexer

		public int this[GoogleReportMetrics m] {
			get { return m_oMetrics.ContainsKey(m) ? m_oMetrics[m] : 0; }
			set { m_oMetrics[m] = value; }
		} // metrics indexer

		private readonly SortedDictionary<GoogleReportDimensions, string> m_oDimensions;
		private readonly SortedDictionary<GoogleReportMetrics, int> m_oMetrics;

	} // class GoogleDataItem

} // namespace EzAnalyticsConsoleClient
