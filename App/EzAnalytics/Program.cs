using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Ezbob.Context;
using Ezbob.Database;
using Ezbob.Logger;
using Google.Apis.Analytics.v3;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using System;
using DotNetOpenAuth.OAuth2;
using System.Collections.Generic;
using System.Globalization;
using EzEnv = Ezbob.Context.Environment;

namespace EzAnalyticsConsoleClient {
	class Program {
		#region method Main

		static void Main(string[] args) {
			var oLog = new LegacyLog();

			var app = new Program(oLog);

			if (app.Environment.Name == Name.Dev) {
				app.Log = new ConsoleLog(app.Log);

				DateTime oDate = DateTime.Today;

				if ((args.Length > 1) && (args[0] == "--date"))
					DateTime.TryParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out oDate);

				if (app.Init(oDate))
					app.Run();

				app.Done();
			}
			else {
				try {
					if (app.Init(DateTime.Today.AddDays(-1)))
						app.Run();

					app.Done();
				}
				catch (Exception ex) {
					app.Log.Error("Error Occured!\n\nStatement: {0}\n\nDescription: {1}", ex.Message, ex.ToString());
					app.Log.Error("\nPress enter to exit");
				} // try
			} // if dev/non-dev env
		} // Main

		#endregion method Main

		#region constructor

		private Program(ASafeLog oLog = null) {
			Log = new SafeLog(oLog);
			Environment = new EzEnv(Log);
		} // constructor

		#endregion constructor

		#region method Init

		private bool Init(DateTime oDate) {
			Log.Debug("Program.Init started...");

			m_oReportDate = oDate;

			Log.Debug("Processing analytics for {0}", m_oReportDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));

			string strKeyFile = Path.Combine(
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
				GoogleDataFetcher.KeyFileName
			);

			Log.Debug("Creating authentication data...");

			var objAuth = ObjGetOAuth2(strKeyFile, string.Empty, GoogleDataFetcher.ServiceAccountUser, GoogleDataFetcher.Scope); //Authentication data returned

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

		#region method Run

		private void Run() {
			Log.Debug("Program.Run started...");

			var oRawByCountry = FetchByCountry();
			var oRawByPage = FetchByPage();

			var oStats = new SortedDictionary<string, int>();
			
			ProcessByCountry(oRawByCountry, oStats);

			ProcessByPage(oRawByPage, oStats);

			SaveStats(oStats);

			var sourceStats = FetchBySource();

			SaveStats(sourceStats);
			Log.Debug("Program.Run complete.");
		} // Run

		#endregion method Run

		#region method Done

		private void Done() {
			Log.Debug("Program.Done started...");
			Log.Debug("Program.Done complete.");
		} // Done

		#endregion method Done

		#region method ProcessByCountry

		private void ProcessByCountry(SortedDictionary<string, CountryData> oRawByCountry, SortedDictionary<string, int> oRes) {
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

		private void ProcessByPage(SortedDictionary<PageID, int> oRawByPage, SortedDictionary<string, int> oRes) {
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

		#region method SaveStats

		private void SaveStats(SortedDictionary<string, int> oStats) {
			Log.Debug("Saving stats started...");

			var conn = new SqlConnection(Log);

			string dbDate = conn.DateToString(m_oReportDate);

			foreach (KeyValuePair<string, int> pair in oStats) {
				conn.ExecuteNonQuery(DbConsts.InsertSiteAnalyticsSP,
					new QueryParameter(DbConsts.IsaspDate, dbDate),
					new QueryParameter(DbConsts.IsaspCodeName, pair.Key),
					new QueryParameter(DbConsts.IsaspValue, pair.Value)
				);
			} // for each stat to save

			Log.Debug("Saving stats complete.");
		} // SaveStats

		private void SaveStats(List<StatsModel> oStats)
		{
			Log.Debug("Saving stats started...");

			var conn = new SqlConnection(Log);

			string dbDate = conn.DateToString(m_oReportDate);

			foreach (var stat in oStats)
			{
				conn.ExecuteNonQuery(DbConsts.InsertSiteAnalyticsSP,
					new QueryParameter(DbConsts.IsaspDate, dbDate),
					new QueryParameter(DbConsts.IsaspCodeName, stat.Code),
					new QueryParameter(DbConsts.IsaspValue, stat.Value),
					new QueryParameter(DbConsts.IsaspSource, stat.Source)
				);
			} // for each stat to save

			Log.Debug("Saving stats complete.");
		}
		#endregion method SaveStats

		#region method FetchByCountry

		private SortedDictionary<string, CountryData> FetchByCountry() {
			Log.Debug("Fetching by country started...");

			var oFetcher = new GoogleDataFetcher(
				m_oService,
				m_oReportDate,
				m_oReportDate, 
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

		private SortedDictionary<PageID, int> FetchByPage() {
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

		private List<StatsModel> FetchBySource()
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

		#region property Environment

		private Ezbob.Context.Environment Environment { get; set; }

		#endregion property Environment

		#region property Log

		private ASafeLog Log { get; set; }

		#endregion property Log

		#region method ObjGetOAuth2

		private OAuth2Authenticator<AssertionFlowClient> ObjGetOAuth2(string strPrivateFilePath, string strPrivateFilePassword, string strServiceAccEmailId, string strScope) {
			string scopeUrl = "https://www.googleapis.com/auth/" + strScope;
			string strSrvAccEmailId = strServiceAccEmailId;
			string strKeyFile = strPrivateFilePath; //KeyFile: This is the physical path to the key file you downloaded when you created your Service Account.
			string strKeyPassword = (strPrivateFilePassword != "") ? strPrivateFilePassword : "notasecret"; //key_pass: This is probably the password for all key files, but if you're given a different one, use that.

			AuthorizationServerDescription objAuthServerDesc = GoogleAuthenticationServer.Description;
			var objKey = new X509Certificate2(strKeyFile, strKeyPassword, X509KeyStorageFlags.Exportable);
			var objClient = new AssertionFlowClient(objAuthServerDesc, objKey) { ServiceAccountId = strSrvAccEmailId, Scope = scopeUrl };
			var objAuth = new OAuth2Authenticator<AssertionFlowClient>(objClient, AssertionFlowClient.GetState);
			return objAuth;
		} // ObjGetOAuth2

		#endregion method ObjGetOAuth2

		private DateTime m_oReportDate;
		private AnalyticsService m_oService;
	} // class Program
} // namespace EzAnalyticsConsoleClient
