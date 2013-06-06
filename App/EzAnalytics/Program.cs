namespace EzAnalyticsConsoleClient
{
	using System.Security.Cryptography.X509Certificates;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Google.Apis.Analytics.v3;
	using Google.Apis.Analytics.v3.Data;
	using Google.Apis.Authentication.OAuth2;
	using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
	using System;
	using DotNetOpenAuth.OAuth2;
	using System.Collections.Generic;
	using Logger;
	using System.Globalization;
	class Program
	{
		static void Main()
		{
			var objOAuth2 = new ClsGetOAuth2Authentication();
			try
			{
				string strKeyFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\08a190d7e7b61e5cdfa63301e528134d3699f096-privatekey.p12";
				string strKeyPassword = string.Empty;
				const string serviceAccountUser = "660066936754-gcs16ckt4dhklhcqrktjispn0l3ddfhl@developer.gserviceaccount.com";
				const string strScope = "analytics";
				//const string strScope = "analytics.readonly";
				var objAuth = objOAuth2.ObjGetOAuth2(strKeyFile, strKeyPassword, serviceAccountUser, strScope); //Authentication data returned
				var initializer = new Google.Apis.Services.BaseClientService.Initializer { Authenticator = objAuth };

				var service = new AnalyticsService(initializer);

				const string profileId = "ga:60953365";
				string startDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");// "2013-03-23";
				string endDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"); // "2013-03-23";

				const string metrics = "ga:visits,ga:visitors";
				DataResource.GaResource.GetRequest request = service.Data.Ga.Get(profileId, startDate, endDate, metrics);
				//var req = new GaData.QueryData();

				request.Dimensions = "ga:date,ga:country";
				GaData data = request.Fetch();
				int all = 0;
				int allunique = 0;
				int ukraine = 0;
				int ukraineUnique = 0;
				int israel = 0;
				int israelUnique = 0;
				int uk = 0;
				int ukUnique = 0;
				string dateStr = null;
				foreach (List<string> row in data.Rows)
				{
					if (row.Count == 4)
					{
						dateStr = row[0];
						all += int.Parse(row[2]);
						allunique += int.Parse(row[3]);
						if (row[1].Equals("Ukraine"))
						{
							ukraine = int.Parse(row[2]);
							ukraineUnique = int.Parse(row[3]);
						}

						if (row[1].Equals("Israel"))
						{
							israel = int.Parse(row[2]);
							israelUnique = int.Parse(row[3]);
						}

						if (row[1].Equals("United Kingdom"))
						{
							uk = int.Parse(row[2]);
							ukUnique = int.Parse(row[3]);
						}
					}
				}

				DateTime date = DateTime.ParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture);

				var log = new LegacyLog();
				var conn = new SqlConnection(log);

				conn.ExecuteNonQuery("InsertSiteAnalytics", 
					new QueryParameter("@Date", date.ToString("yyyy-MM-dd")),
					new QueryParameter("@CodeName", Consts.UkVisitis),
					new QueryParameter("@Value", uk));
				conn.ExecuteNonQuery("InsertSiteAnalytics",
					new QueryParameter("@Date", date.ToString("yyyy-MM-dd")),
					new QueryParameter("@CodeName", Consts.UkVisitors),
					new QueryParameter("@Value", ukUnique));
				conn.ExecuteNonQuery("InsertSiteAnalytics",
					new QueryParameter("@Date", date.ToString("yyyy-MM-dd")),
					new QueryParameter("@CodeName", Consts.WorldWideVisitis),
					new QueryParameter("@Value", all - ukraine - israel));
				conn.ExecuteNonQuery("InsertSiteAnalytics",
					new QueryParameter("@Date", date.ToString("yyyy-MM-dd")),
					new QueryParameter("@CodeName", Consts.WorldWideVisitors),
					new QueryParameter("@Value", allunique - ukraineUnique - israelUnique));

				Logger.DebugFormat("all:{0},all unique:{1}, uk:{2}, uk unique:{3}, ukraine:{4}, ukraine unique:{5}, israel:{6}, israel unique:{7}",
					all, allunique, uk, ukUnique, ukraine, ukraineUnique, israel, israelUnique);
				Logger.DebugFormat("all(without ukraine,israel):{0},all unique(without ukraine,israel):{1}, uk:{2}, uk unique:{3}", all - ukraine - israel, allunique - ukraineUnique - israelUnique, uk, ukUnique);

			}
			catch (Exception ex)
			{
				Logger.DebugFormat("Error Occured!\n\nStatement:- {0}\n\nDescription:-{1}", ex.Message, ex.ToString());
				Logger.Debug("\nPress enter to exit");
			}
		}

	}

	internal class ClsGetOAuth2Authentication
	{
		public OAuth2Authenticator<AssertionFlowClient> ObjGetOAuth2(string strPrivateFilePath, string strPrivateFilePassword, string strServiceAccEmailId, string strScope)
		{
			string scopeUrl = "https://www.googleapis.com/auth/" + strScope;
			string strSrvAccEmailId = strServiceAccEmailId;
			string strKeyFile = strPrivateFilePath; //KeyFile: This is the physical path to the key file you downloaded when you created your Service Account.
			string strKeyPassword = (strPrivateFilePassword != "") ? strPrivateFilePassword : "notasecret"; //key_pass: This is probably the password for all key files, but if you're given a different one, use that.

			AuthorizationServerDescription objAuthServerDesc = GoogleAuthenticationServer.Description;
			var objKey = new X509Certificate2(strKeyFile, strKeyPassword, X509KeyStorageFlags.Exportable);
			var objClient = new AssertionFlowClient(objAuthServerDesc, objKey) { ServiceAccountId = strSrvAccEmailId, Scope = scopeUrl };
			var objAuth = new OAuth2Authenticator<AssertionFlowClient>(objClient, AssertionFlowClient.GetState);
			return objAuth;
		}
	}
}