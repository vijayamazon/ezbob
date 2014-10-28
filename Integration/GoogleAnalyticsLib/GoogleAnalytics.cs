namespace GoogleAnalyticsLib  {
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
		
		public void SetDate(DateTime date)
		{
			m_oReportDate = date;
			Log.Debug("Processing analytics for {0}", m_oReportDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));
		}

		#region method ProcessByCountry

		public void ProcessByCountry(SortedDictionary<string, CountryData> oRawByCountry, SortedDictionary<string, int> oRes) {
			int nAllUsers = oRawByCountry.Select(x => x.Value.Users).Sum();

			int nNewUsers = oRawByCountry.Select(x => x.Value.NewUsers).Sum();

			int nReturningUsers = oRawByCountry.Select(x => x.Value.Returning).Sum();

			Log.Debug("Total unique Users: {0}", nAllUsers);
			Log.Debug("Total new Users: {0}", nNewUsers);
			Log.Debug("Total returning Users: {0}", nReturningUsers);

			oRes[DbConsts.UkUsers] = Users(oRawByCountry, GoogleDataFetcher.UK);
			oRes[DbConsts.UkNewUsers] = NewUsers(oRawByCountry, GoogleDataFetcher.UK);
			oRes[DbConsts.UkReturningUsers] = Users(oRawByCountry, GoogleDataFetcher.UK) - NewUsers(oRawByCountry, GoogleDataFetcher.UK);

			oRes[DbConsts.WorldWideUsers] = nAllUsers -
				Users(oRawByCountry, GoogleDataFetcher.IL) -
				Users(oRawByCountry, GoogleDataFetcher.UA);

			oRes[DbConsts.WorldWideNewUsers] = nNewUsers -
				NewUsers(oRawByCountry, GoogleDataFetcher.IL) -
				NewUsers(oRawByCountry, GoogleDataFetcher.UA);

			oRes[DbConsts.WorldWideReturningUsers] = nReturningUsers;

		} // ProcessByCountry

		#endregion method ProcessByCountry

		#region method ProcessByPage

		public void ProcessByPage(SortedDictionary<PageID, int> oRawByPage, SortedDictionary<string, int> oRes) {
			foreach (KeyValuePair<PageID, int> pair in oRawByPage)
				oRes[DbConsts.Page + pair.Key.ToString()] = pair.Value;
		} // ProcessByPage

		#endregion method ProcessByPage

		#region method Users

		private int Users(SortedDictionary<string, CountryData> oRawStats, string sCountry) {
			if (oRawStats.ContainsKey(sCountry))
				return oRawStats[sCountry].Users;

			return 0;
		} // Users

		#endregion method Users

		#region method NewUsers

		private int NewUsers(SortedDictionary<string, CountryData> oRawStats, string sCountry)
		{
			if (oRawStats.ContainsKey(sCountry))
				return oRawStats[sCountry].NewUsers;

			return 0;
		} // NewUsers

		#endregion method NewUsers

		#region method FetchByCountry

		public SortedDictionary<string, CountryData> FetchByCountry(DateTime startDate, DateTime endDate) {
			Log.Debug("Fetching by country started...");

			var oFetcher = new GoogleDataFetcher(
				m_oService,
				startDate,
				endDate, 
				new GoogleReportDimensions[] { GoogleReportDimensions.country, },
				new GoogleReportMetrics[] { GoogleReportMetrics.users, GoogleReportMetrics.newUsers },
				null
				/*GoogleDataFetcher.GAString(GoogleReportDimensions.hostname) + "==ezbob.com"*/,
				Log
			);

			List<GoogleDataItem> oFetchResult = oFetcher.Fetch();

			var oByCountry = new SortedDictionary<string, CountryData>();

			foreach (GoogleDataItem oItem in oFetchResult) {
				string sCountry = oItem[GoogleReportDimensions.country];
				int nUsers = oItem[GoogleReportMetrics.users];
				int nNewUsers = oItem[GoogleReportMetrics.newUsers];

				if (oByCountry.ContainsKey(sCountry))
					oByCountry[sCountry].Add(nUsers, nNewUsers);
				else
					oByCountry[sCountry] = new CountryData(nUsers, nNewUsers);
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
				new GoogleReportMetrics[] { GoogleReportMetrics.users },
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

				int nUsers = oItem[GoogleReportMetrics.users];

				if (oByPage.ContainsKey(nPageID))
					oByPage[nPageID] += nUsers;
				else
					oByPage[nPageID] = nUsers;
			} // for each item

			Log.Debug("By page - begin");

			foreach (KeyValuePair<PageID, int> pair in oByPage)
				Log.Debug("{1} to {0}", pair.Key, pair.Value);

			Log.Debug("By page - end");

			Log.Debug("Fetching by page complete.");

			return oByPage;
		} // FetchByPage

		#endregion method FetchByPage

		#region method FetchBySource

		public List<StatsModel> FetchBySource()
		{
			Log.Debug("Fetching by source started...");

			var oFetcher = new GoogleDataFetcher(
				m_oService,
				m_oReportDate,
				m_oReportDate,
				new GoogleReportDimensions[] { GoogleReportDimensions.sourceMedium, GoogleReportDimensions.country, },
				new GoogleReportMetrics[] { GoogleReportMetrics.users, GoogleReportMetrics.newUsers },
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

				int nUsers = oItem[GoogleReportMetrics.users];

				int nNewUsers = oItem[GoogleReportMetrics.newUsers];

				model.Add(new StatsModel {
					CodeName = DbConsts.SourceUsers,
					Source = source,
					Value = nUsers
				});

				model.Add(new StatsModel {
					CodeName = DbConsts.SourceNewUsers,
					Source = source,
					Value = nNewUsers
				});
					
				Log.Debug("source: {0}, Users: {1}, new Users: {2}", source, nUsers, nNewUsers);
			} // for each item

			Log.Debug("Fetching by source complete.");

			return model;
		} // FetchByPage

		#endregion method FetchBySource

		#region method FetchByLandingPage

		public List<StatsModel> FetchByLandingPage()
		{
			Log.Debug("Fetching by source started...");

			var oFetcher = new GoogleDataFetcher(
				m_oService,
				m_oReportDate,
				m_oReportDate,
				new GoogleReportDimensions[] { GoogleReportDimensions.landingPagePath },
				new GoogleReportMetrics[] { GoogleReportMetrics.users, GoogleReportMetrics.newUsers },
				null,
				Log
			);
			var model = new List<StatsModel>(); 
			List<GoogleDataItem> oFetchResult = oFetcher.Fetch();

			foreach (GoogleDataItem oItem in oFetchResult)
			{
				string landingPagePath = oItem[GoogleReportDimensions.landingPagePath];

				int nUsers = oItem[GoogleReportMetrics.users];

				int nNewUsers = oItem[GoogleReportMetrics.newUsers];

				model.Add(new StatsModel {
					CodeName = DbConsts.LandingPageUsers,
					Source = landingPagePath,
					Value = nUsers
				});

				model.Add(new StatsModel {
					CodeName = DbConsts.LandingPageNewUsers,
					Source = landingPagePath,
					Value = nNewUsers
				});

				Log.Debug("landingPagePath: {0}, Users: {1}, new Users: {2}", landingPagePath, nUsers, nNewUsers);
			} // for each item

			Log.Debug("Fetching by source complete.");

			return model;
		} // FetchByPage

		#endregion method FetchBySource

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
				Log.Error("GA Certificate not found by thumb '{0}'", thumb);
				throw new Exception("Certificate not found");
			}

			Log.Debug("certs found {0}, first: {1}", cert.Count, cert[0].FriendlyName);

			Log.Debug("strServiceAccEmailId = '{0}', strScope = '{1}'", strServiceAccEmailId, strScope);

			var objClient = new AssertionFlowClient(objAuthServerDesc, cert[0]) { ServiceAccountId = strSrvAccEmailId, Scope = scopeUrl };

			var objAuth = new OAuth2Authenticator<AssertionFlowClient>(objClient, AssertionFlowClient.GetState);
			return objAuth;
		} // ObjGetOAuth2

		#endregion method ObjGetOAuth2

		private DateTime m_oReportDate;
		private AnalyticsService m_oService;
	} // class Program
} // namespace EzAnalyticsConsoleClient
