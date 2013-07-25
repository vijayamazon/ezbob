using System.IO;
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

			SortedDictionary<string, CountryData> oRawStats = FetchStats();

			SortedDictionary<string, int> oStats = ProcessStats(oRawStats);

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

		#region method ProcessStats

		private SortedDictionary<string, int> ProcessStats(SortedDictionary<string, CountryData> oRawStats) {
			var oRes = new SortedDictionary<string, int>();

			int nAllVisits = 0;
			int nAllVisitors = 0;

			foreach (KeyValuePair<string, CountryData> oCtrData in oRawStats) {
				nAllVisits += oCtrData.Value.Visits;
				nAllVisitors += oCtrData.Value.Visitors;
			} // for each country

			Log.Debug("Total visits: {0}", nAllVisits);
			Log.Debug("Total visitors: {0}", nAllVisitors);

			oRes[DbConsts.UkVisits] = Visits(oRawStats, GoogleConsts.UK);
			oRes[DbConsts.UkVisitors] = Visitors(oRawStats, GoogleConsts.UK);

			oRes[DbConsts.WorldWideVisits] = nAllVisits - Visits(oRawStats, GoogleConsts.IL) - Visits(oRawStats, GoogleConsts.UA);
			oRes[DbConsts.WorldWideVisitors] = nAllVisitors - Visitors(oRawStats, GoogleConsts.IL) - Visitors(oRawStats, GoogleConsts.UA);

			return oRes;
		} // ProcessStats

		#endregion method ProcessStats

		#region method Visits

		private int Visits(SortedDictionary<string, CountryData> oRawStats, string sCountry) {
			if (oRawStats.ContainsKey(sCountry))
				return oRawStats[sCountry].Visits;

			return 0;
		} // Visits

		#endregion method Visits

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
				conn.ExecuteNonQuery("InsertSiteAnalytics",
					new QueryParameter("@Date", dbDate),
					new QueryParameter("@CodeName", pair.Key),
					new QueryParameter("@Value", pair.Value)
				);
			} // for each stat to save

			Log.Debug("Saving stats complete.");
		} // SaveStats

		#endregion method SaveStats

		#region method FetchStats

		private SortedDictionary<string, CountryData> FetchStats() {
			Log.Debug("Fetching stats started...");

			var oRes = new SortedDictionary<string, CountryData>();

			string sDate = m_oReportDate.ToString(GoogleConsts.OAuthDateFormat, CultureInfo.InvariantCulture);

			Log.Debug("Creating request...");

			DataResource.GaResource.GetRequest request = m_oService.Data.Ga.Get(GoogleConsts.ProfileID, sDate, sDate, GoogleConsts.Metrics);

			//to retrieve accounts: Accounts accounts = service.Management.Accounts.List().Fetch();
			//to retrieve profiles: var profiles = service.Management.Profiles.List("32583191", "UA-32583191-1").Fetch();
			/*foreach (Profile profile in profiles.Items){Console.WriteLine("Profile Timezone: " + profile.Timezone);}*/

			request.Dimensions = GoogleConsts.Dimensions;

			Log.Debug("Fetching...");

			GaData data = request.Fetch();

			Log.Debug("Processing...");

			foreach (List<string> pkg in data.Rows) {
				if (pkg.Count != 4)
					continue;

				string sCountry = pkg[1];
				int nVisits = int.Parse(pkg[2]);
				int nVisitors = int.Parse(pkg[3]);

				if (oRes.ContainsKey(sCountry)) {
					oRes[sCountry].Add(nVisits, nVisitors);
					Log.Debug("First time country: {0}, {1} visits, {2} visitors", sCountry, nVisits, nVisitors);
				}
				else {
					oRes[sCountry] = new CountryData(nVisits, nVisitors);
					Log.Debug("Repeating country: {0}, {1} visits, {2} visitors", sCountry, nVisits, nVisitors);
				} // if
			} // for each package

			Log.Debug("Fetching stats complete.");

			return oRes;
		} // FetchStats

		#endregion method FetchStats

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
