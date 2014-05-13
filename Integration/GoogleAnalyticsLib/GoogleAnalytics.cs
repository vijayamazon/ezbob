namespace GoogleAnalyticsLib  {
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;
	using Ezbob.Logger;
	using Google.Apis.Analytics.v3;
	using Google.Apis.Authentication.OAuth2;
	using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
	using System;
	using DotNetOpenAuth.OAuth2;
	using System.Collections.Generic;
	using System.Globalization;

	public class GoogleAnalytics {

		#region constructor

		public GoogleAnalytics(ASafeLog oLog = null)
		{
			Log = new SafeLog(oLog);
		} // constructor

		#endregion constructor

		#region method Init

		public bool Init(DateTime oDate, string thumb) {
			Log.Debug("Program.Init started...");

			m_oReportDate = oDate;

			Log.Debug("Processing analytics for {0}", m_oReportDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));
			
			Log.Debug("Creating authentication data...");
			
			var objAuth = ObjGetOAuth2(thumb, GoogleDataFetcher.ServiceAccountUser, GoogleDataFetcher.Scope); //Authentication data returned

			Log.Debug("Creating initialser...");

			var initializer = new Google.Apis.Services.BaseClientService.Initializer {
				Authenticator = objAuth
			};

			Log.Debug("Creating service client...");

			m_oService = new AnalyticsService(initializer);
			
			Log.Debug("Program.Init complete.");

			return true;
		} // Init

		#endregion method Init

		#region method ProcessByCountry

		public void ProcessByCountry(SortedDictionary<string, CountryData> oRawByCountry, SortedDictionary<string, int> oRes) {
			int nAllVisitors = oRawByCountry.Select(x => x.Value.Visitors).Sum();

			int nNewVisitors = oRawByCountry.Select(x => x.Value.New).Sum();

			int nReturningVisitors = oRawByCountry.Select(x => x.Value.Returning).Sum();

			Log.Debug("Total unique visitors: {0}", nAllVisitors);
			Log.Debug("Total new visitors: {0}", nNewVisitors);
			Log.Debug("Total returning visitors: {0}", nReturningVisitors);

			oRes[DbConsts.UkVisitors] = Visitors(oRawByCountry, GoogleDataFetcher.UK);

			oRes[DbConsts.WorldWideVisitors] = nAllVisitors -
				Visitors(oRawByCountry, GoogleDataFetcher.IL) -
				Visitors(oRawByCountry, GoogleDataFetcher.UA);

			oRes[DbConsts.NewVisitors] = nNewVisitors;

			oRes[DbConsts.ReturningVisitors] = nReturningVisitors;
		} // ProcessByCountry

		#endregion method ProcessByCountry

		#region method ProcessByPage

		public void ProcessByPage(SortedDictionary<PageID, int> oRawByPage, SortedDictionary<string, int> oRes) {
			foreach (KeyValuePair<PageID, int> pair in oRawByPage)
				oRes[DbConsts.Page + pair.Key.ToString()] = pair.Value;
		} // ProcessByPage

		#endregion method ProcessByPage

		#region method Visitors

		private int Visitors(SortedDictionary<string, CountryData> oRawStats, string sCountry) {
			if (oRawStats.ContainsKey(sCountry))
				return oRawStats[sCountry].Visitors;

			return 0;
		} // Visitors

		#endregion method Visitors
		
		#region method FetchByCountry

		public SortedDictionary<string, CountryData> FetchByCountry(DateTime startDate, DateTime endDate) {
			Log.Debug("Fetching by country started...");

			var oFetcher = new GoogleDataFetcher(
				m_oService,
				startDate,
				endDate, 
				new GoogleReportDimensions[] { GoogleReportDimensions.country, },
				new GoogleReportMetrics[] { GoogleReportMetrics.visitors, GoogleReportMetrics.newVisits },
				null
				/*GoogleDataFetcher.GAString(GoogleReportDimensions.hostname) + "==ezbob.com"*/,
				Log
			);

			List<GoogleDataItem> oFetchResult = oFetcher.Fetch();

			var oByCountry = new SortedDictionary<string, CountryData>();

			foreach (GoogleDataItem oItem in oFetchResult) {
				string sCountry = oItem[GoogleReportDimensions.country];
				int nVisitors = oItem[GoogleReportMetrics.visitors];
				int nNewVisitors = oItem[GoogleReportMetrics.newVisits];

				if (oByCountry.ContainsKey(sCountry))
					oByCountry[sCountry].Add(nVisitors, nNewVisitors);
				else
					oByCountry[sCountry] = new CountryData(nVisitors, nNewVisitors);
			} // for each item

			Log.Debug("Fetched by country - begin");

			foreach (KeyValuePair<string, CountryData> pair in oByCountry)
				Log.Debug("{0}: {1}", pair.Key, pair.Value);

			Log.Debug("Fetched by country - end");

			Log.Debug("Fetching by country complete.");

			return oByCountry;
		} // FetchByCountry

		#endregion method FetchByCountry

		#region method FetchByPage

		public SortedDictionary<PageID, int> FetchByPage() {
			Log.Debug("Fetching by page started...");

			var oFetcher = new GoogleDataFetcher(
				m_oService,
				m_oReportDate,
				m_oReportDate, 
				new GoogleReportDimensions[] { GoogleReportDimensions.pagePath, },
				new GoogleReportMetrics[] { GoogleReportMetrics.visitors },
				null
				/*GoogleDataFetcher.GAString(GoogleReportDimensions.hostname) + "==ezbob.com"*/,
				Log
			);

			List<GoogleDataItem> oFetchResult = oFetcher.Fetch();

			var oByPage = new SortedDictionary<PageID, int>();

			foreach (GoogleDataItem oItem in oFetchResult) {
				string sPagePath = oItem[GoogleReportDimensions.pagePath];

				PageID nPageID = GetPageID(sPagePath);

				if (nPageID == PageID.Other)
					continue;

				int nVisitors = oItem[GoogleReportMetrics.visitors];

				if (oByPage.ContainsKey(nPageID))
					oByPage[nPageID] += nVisitors;
				else
					oByPage[nPageID] = nVisitors;
			} // for each item

			Log.Debug("By page - begin");

			foreach (KeyValuePair<PageID, int> pair in oByPage)
				Log.Debug("{1} to {0}", pair.Key, pair.Value);

			Log.Debug("By page - end");

			Log.Debug("Fetching by page complete.");

			return oByPage;
		} // FetchByPage

		#endregion method FetchByPage

		#region method FetchByPage

		public List<StatsModel> FetchBySource()
		{
			Log.Debug("Fetching by source started...");

			var oFetcher = new GoogleDataFetcher(
				m_oService,
				m_oReportDate,
				m_oReportDate,
				new GoogleReportDimensions[] { GoogleReportDimensions.sourceMedium, GoogleReportDimensions.country, },
				new GoogleReportMetrics[] { GoogleReportMetrics.visitors, GoogleReportMetrics.newVisits },
				null
				/*GoogleDataFetcher.GAString(GoogleReportDimensions.hostname) + "==ezbob.com"*/, 
				Log
			);
			var model = new List<StatsModel>(); 
			List<GoogleDataItem> oFetchResult = oFetcher.Fetch();

			foreach (GoogleDataItem oItem in oFetchResult)
			{
				string country = oItem[GoogleReportDimensions.country];
				if (country != GoogleDataFetcher.UK)
				{
					continue;
				}

				string source = oItem[GoogleReportDimensions.sourceMedium];

				int nVisitors = oItem[GoogleReportMetrics.visitors];

				int nNewVisits = oItem[GoogleReportMetrics.newVisits];

				model.Add(new StatsModel
					{
						Code = DbConsts.SourceVisitors,
						Source = source,
						Value = nVisitors
					});
				model.Add(new StatsModel
					{
						Code = DbConsts.SourceVisits,
						Source = source,
						Value = nNewVisits
					});
					
				Log.Debug("source: {0}, visitors: {1}, new visits: {2}", source, nVisitors, nNewVisits);
			} // for each item

			Log.Debug("Fetching by source complete.");

			return model;
		} // FetchByPage

		#endregion method FetchByPage

		#region method GetPageID

		private PageID GetPageID(string sPagePath) {
			if (sPagePath == "/Account/LogOn")
				return PageID.Logon;

			if (sPagePath == "/Customer/PacnetStatus")
				return PageID.Pacnet;

			if (sPagePath == "/Customer/Profile/GetCash")
				return PageID.GetCash;

			if (sPagePath == "/Customer/Profile")
				return PageID.Dashboard;

			return PageID.Other;
		} // GetPageID

		#endregion method GetPageID

		#region property Log

		public ASafeLog Log { get; set; }

		#endregion property Log

		#region method ObjGetOAuth2

		private OAuth2Authenticator<AssertionFlowClient> ObjGetOAuth2(string thumb, string strServiceAccEmailId, string strScope) {
			string scopeUrl = "https://www.googleapis.com/auth/" + strScope;
			string strSrvAccEmailId = strServiceAccEmailId;
			
			AuthorizationServerDescription objAuthServerDesc = GoogleAuthenticationServer.Description;
			
			var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
			store.Open(OpenFlags.ReadOnly);
			var cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumb, false);
			if (cert.Count == 0)
			{
				Log.Error("GA Certificate not found");
				throw new Exception("Certificate not found");
			}

			Log.Debug("certs found {0}, first: {1}", cert.Count, cert[0].FriendlyName);
			var objClient = new AssertionFlowClient(objAuthServerDesc, cert[0]) { ServiceAccountId = strSrvAccEmailId, Scope = scopeUrl };
			var objAuth = new OAuth2Authenticator<AssertionFlowClient>(objClient, AssertionFlowClient.GetState);
			return objAuth;
		} // ObjGetOAuth2

		#endregion method ObjGetOAuth2

		private DateTime m_oReportDate;
		private AnalyticsService m_oService;
	} // class Program
} // namespace EzAnalyticsConsoleClient
