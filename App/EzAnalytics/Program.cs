using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Ezbob.Context;
using Ezbob.Database;
using Ezbob.Logger;
using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;
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

		static void Main() {
			var oLog = new LegacyLog();

			var app = new Program(oLog);

			if (app.Environment.Name == Name.Dev) {
				app.Log = new ConsoleLog(app.Log);

				if (app.Init())
					app.Run();

				app.Done();
			}
			else {
				try {
					if (app.Init())
						app.Run();

					app.Done();
				}
				catch (Exception ex) {
					app.Log.Error("Error Occured!\n\nStatement:- {0}\n\nDescription:-{1}", ex.Message, ex.ToString());
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

		private bool Init() {
			Log.Debug("Program.Init started...");

			m_oReportDate = DateTime.Today;

			Log.Debug("Processing analytics for {0}", m_oReportDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));

			string strKeyFile = Path.Combine(
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
				GoogleConsts.KeyFileName
			);

			Log.Debug("Creating authentication data...");

			var objAuth = ObjGetOAuth2(strKeyFile, string.Empty, GoogleConsts.ServiceAccountUser, GoogleConsts.Scope); //Authentication data returned

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

			var oRawByCountry = new SortedDictionary<string, CountryData>();
			var oRawByPage = new SortedDictionary<PageID, int>();
			
			FetchStats(oRawByCountry, oRawByPage);

			var oStats = new SortedDictionary<string, int>();
			
			ProcessByCountry(oRawByCountry, oStats);

			ProcessByPage(oRawByPage, oStats);

			SaveStats(oStats);

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

			oRes[DbConsts.UkVisitors] = Visitors(oRawByCountry, GoogleConsts.UK);

			oRes[DbConsts.WorldWideVisitors] = nAllVisitors - Visitors(oRawByCountry, GoogleConsts.IL) - Visitors(oRawByCountry, GoogleConsts.UA);

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

		#endregion method SaveStats

		#region method FetchStats

		private void FetchStats(SortedDictionary<string, CountryData> oByCountry, SortedDictionary<PageID, int> oByPage) {
			Log.Debug("Fetching stats started...");

			string sDate = m_oReportDate.ToString(GoogleConsts.OAuthDateFormat, CultureInfo.InvariantCulture);

			Log.Debug("Creating request...");

			var oGoogleConsts = new GoogleConsts();

			DataResource.GaResource.GetRequest request = m_oService.Data.Ga.Get(GoogleConsts.ProfileID, sDate, sDate, oGoogleConsts.Metrics);

			//to retrieve accounts: Accounts accounts = service.Management.Accounts.List().Fetch();
			//to retrieve profiles: var profiles = service.Management.Profiles.List("32583191", "UA-32583191-1").Fetch();
			/*foreach (Profile profile in profiles.Items){Console.WriteLine("Profile Timezone: " + profile.Timezone);}*/

			request.Dimensions = oGoogleConsts.Dimensions;

			Log.Debug("Fetching with dimensions {0} and metrics {1}...", oGoogleConsts.Dimensions, oGoogleConsts.Metrics);

			GaData data = request.Fetch();

			Log.Debug("Processing...");

			foreach (List<string> pkg in data.Rows) {
				if (pkg.Count != oGoogleConsts.DimMetCount)
					continue;

				// Log.Debug("\n\n\nBegin\n\n\n"); foreach (string s in pkg) Log.Debug("-- row: {0}", s); Log.Debug("\n\n\nEnd\n\n\n");

				string sCountry = pkg[oGoogleConsts.Idx(GoogleReportDimensions.country)];
				string sHostName = pkg[oGoogleConsts.Idx(GoogleReportDimensions.hostname)];
				string sPagePath = pkg[oGoogleConsts.Idx(GoogleReportDimensions.pagePath)];
				int nVisitors = int.Parse(pkg[oGoogleConsts.Idx(GoogleReportMetrics.visitors)]);
				int nNewVisitors = int.Parse(pkg[oGoogleConsts.Idx(GoogleReportMetrics.newVisits)]);

				if (oByCountry.ContainsKey(sCountry))
					oByCountry[sCountry].Add(nVisitors, nNewVisitors);
				else
					oByCountry[sCountry] = new CountryData(nVisitors, nNewVisitors);

				PageID nPageID = GetPageID(sHostName, sPagePath);

				if (nPageID != PageID.Other) {
					if (oByPage.ContainsKey(nPageID))
						oByPage[nPageID] += nVisitors;
					else
						oByPage[nPageID] = nVisitors;
				} // if

				Log.Debug("Country: {0} visitors: {1} to {2}{3}", sCountry, nVisitors, sHostName, sPagePath);
			} // for each package

			Log.Debug("Fetching stats complete.");
		} // FetchStats

		#endregion method FetchStats

		#region method GetPageID

		private PageID GetPageID(string sHostName, string sPagePath) {
			if (sHostName != "app.ezbob.com")
				return PageID.Other;

			if (sPagePath == "/Account/LogOn")
				return PageID.Logon;

			if (sPagePath == "/Customer/PacnetStatus")
				return PageID.Pacnet;

			if (sPagePath == "/Customer/Profile/GetCash")
				return PageID.GetCash;

			if (sPagePath.StartsWith("/Customer/Profile"))
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
