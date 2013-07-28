using System;
using System.Collections.Generic;
using System.Linq;

namespace EzAnalyticsConsoleClient {
	#region class GoogleConsts

	class GoogleConsts {
		#region public

		#region constants

		public const string ProfileID = "ga:60953365";
		public const string KeyFileName = "08a190d7e7b61e5cdfa63301e528134d3699f096-privatekey.p12";
		public const string ServiceAccountUser = "660066936754-gcs16ckt4dhklhcqrktjispn0l3ddfhl@developer.gserviceaccount.com";
		public const string Scope = "analytics";

		public const string OAuthDateFormat = "yyyy-MM-dd";

		public const string IL = "Israel";
		public const string UA = "Ukraine";
		public const string UK = "United Kingdom";

		#endregion constants

		#region constructor

		public GoogleConsts() {
			m_oDimensions = new SortedDictionary<GoogleReportDimensions, int>();
			m_oMetrics = new SortedDictionary<GoogleReportMetrics, int>();

			m_sDimensions = FillIndices(m_oDimensions);
			m_sMetrics = FillIndices(m_oMetrics);
		} // constructor

		#endregion constructor

		#region property Dimensions

		public string Dimensions { get { return m_sDimensions; } } // Dimensions

		private string m_sDimensions;

		#endregion property Dimensions

		#region property Metrics

		public string Metrics { get { return m_sMetrics; } } // Metrics

		private string m_sMetrics;

		#endregion property Metrics

		#region method Idx

		public int Idx(GoogleReportDimensions val) { return m_oDimensions[val]; } // Idx

		public int Idx(GoogleReportMetrics val) { return m_oDimensions.Count + m_oMetrics[val]; } // Idx

		#endregion method Idx

		#region property DimMetCount

		public int DimMetCount { get { return m_oDimensions.Count + m_oMetrics.Count; } }

		#endregion property DimMetCount

		#endregion public

		#region private

		private readonly SortedDictionary<GoogleReportDimensions, int> m_oDimensions;
		private readonly SortedDictionary<GoogleReportMetrics, int> m_oMetrics;

		#region method FillIndices

		private string FillIndices<T>(SortedDictionary<T, int> oDic) {
			T[] aryVal = (T[])Enum.GetValues(typeof(T));

			for (int i = 0; i < aryVal.Length; i++)
				oDic[aryVal[i]] = i;

			return string.Join(",", aryVal.Select(x => "ga:" + x.ToString()).ToArray());
		} // FillIndices

		#endregion method FillIndices

		#endregion private
	} // class GoogleConsts

	#endregion class GoogleConsts
} // namespace EzAnalyticsConsoleClient
