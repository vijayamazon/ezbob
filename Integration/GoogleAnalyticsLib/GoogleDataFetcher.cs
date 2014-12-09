using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Ezbob.Logger;
using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;

namespace GoogleAnalyticsLib
{

	class GoogleDataFetcher : SafeLog {

		public const string ProfileID = "ga:60953365";
		public const string KeyFileName = "08a190d7e7b61e5cdfa63301e528134d3699f096-privatekey.p12";
		public const string ServiceAccountUser = "660066936754-gcs16ckt4dhklhcqrktjispn0l3ddfhl@developer.gserviceaccount.com";
		public const string Scope = "analytics";

		public const string OAuthDateFormat = "yyyy-MM-dd";

		public const string IL = "Israel";
		public const string UA = "Ukraine";
		public const string UK = "United Kingdom";

		public GoogleDataFetcher(AnalyticsService oService, DateTime oStartDate, DateTime oEndDate, IEnumerable<GoogleReportDimensions> oDimensions, IEnumerable<GoogleReportMetrics> oMetrics, string sFilters = null, ASafeLog oLog = null) : base(oLog) {
			m_oService = oService;
			m_oStartDate = oStartDate;
			m_oEndDate = oEndDate;

			m_oDimensions = new SortedDictionary<GoogleReportDimensions, int>();
			m_oMetrics = new SortedDictionary<GoogleReportMetrics, int>();

			Dimensions = FillIndices(m_oDimensions, oDimensions);
			Metrics = FillIndices(m_oMetrics, oMetrics);

			m_sFilters = string.IsNullOrWhiteSpace(sFilters) ? null : sFilters;
		} // constructor

		public List<GoogleDataItem> Fetch() {
			string sStartDate = m_oStartDate.ToString(GoogleDataFetcher.OAuthDateFormat, CultureInfo.InvariantCulture);
			string sEndDate = m_oEndDate.ToString(GoogleDataFetcher.OAuthDateFormat, CultureInfo.InvariantCulture);

			DataResource.GaResource.GetRequest request = m_oService.Data.Ga.Get(GoogleDataFetcher.ProfileID, sStartDate, sEndDate, Metrics);

			//to retrieve accounts: Accounts accounts = service.Management.Accounts.List().Fetch();
			//to retrieve profiles: var profiles = service.Management.Profiles.List("32583191", "UA-32583191-1").Fetch();
			/*foreach (Profile profile in profiles.Items){Console.WriteLine("Profile Timezone: " + profile.Timezone);}*/

			request.Dimensions = Dimensions;

			if (m_sFilters != null)
				request.Filters = m_sFilters;

			Debug("Fetching for {2} - {3} with dimensions {0} and metrics {1}...", Dimensions, Metrics, sStartDate, sEndDate);

			GaData data = request.Fetch();

			Debug("Processing...");

			var oOutput = new List<GoogleDataItem>();

			foreach (List<string> pkg in data.Rows) {
				if (pkg.Count != DimMetCount)
					continue;

				Debug("-- Begin"); foreach (string s in pkg) Debug("-- row: {0}", s); Debug("-- End");

				var oItem = new GoogleDataItem();

				foreach (KeyValuePair<GoogleReportDimensions, int> pair in m_oDimensions)
					oItem[pair.Key] = pkg[Idx(pair.Key)];

				foreach (KeyValuePair<GoogleReportMetrics, int> pair in m_oMetrics)
					oItem[pair.Key] = int.Parse(pkg[Idx(pair.Key)]);

				oOutput.Add(oItem);
			} // for each package

			Debug("Fetching stats complete.");

			return oOutput;
		} // Fetch

		public static string GAString<T>(T v) {
			return string.Format("ga:{0}", v);
		} // GAString

		private string Dimensions { get; set; }

		private string Metrics { get; set; }

		private int Idx(GoogleReportDimensions val) { return m_oDimensions[val]; } // Idx

		private int Idx(GoogleReportMetrics val) { return m_oDimensions.Count + m_oMetrics[val]; } // Idx

		private int DimMetCount { get { return m_oDimensions.Count + m_oMetrics.Count; } }

		private string FillIndices<T>(SortedDictionary<T, int> oDic, IEnumerable<T> oItems) {
			int i = 0;

			var sb = new StringBuilder();

			foreach (T oi in oItems) {
				oDic[oi] = i;

				if (i != 0)
					sb.Append(",");

				sb.Append(GAString(oi));

				i++;
			} // for each

			return sb.ToString();
		} // FillIndices

		private readonly AnalyticsService m_oService;
		private DateTime m_oStartDate;
		private DateTime m_oEndDate;

		private string m_sFilters;

		private readonly SortedDictionary<GoogleReportDimensions, int> m_oDimensions;
		private readonly SortedDictionary<GoogleReportMetrics, int> m_oMetrics;

	} // class GoogleDataFetcher

} // namespace EzAnalyticsConsoleClient
