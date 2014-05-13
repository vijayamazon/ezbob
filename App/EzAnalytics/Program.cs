using Ezbob.Context;
using Ezbob.Database;
using Ezbob.Logger;
using System;
using System.Collections.Generic;
using System.Globalization;
using EzEnv = Ezbob.Context.Environment;

namespace EzAnalyticsConsoleClient {
	using GoogleAnalyticsLib;

	class Program {
		#region method Main

		static void Main(string[] args) {
			var oLog = new LegacyLog();
			var app = new GoogleAnalytics(oLog);
			Environment = new EzEnv(oLog);
			string thumb = System.Configuration.ConfigurationManager.AppSettings["gaCertThumb"];
			
			if (Environment.Name == Name.Dev) {
				Log = new ConsoleLog(oLog);

				DateTime oDate = DateTime.Today;

				if ((args.Length > 1) && (args[0] == "--date"))
					DateTime.TryParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out oDate);

				
				if (app.Init(oDate, thumb))
				{
					m_oReportDate = oDate;
					Run(app);
				}
					

				Done();
			}
			else {
				try {
					if (app.Init(DateTime.Today, thumb))
						Run(app);

					Done();
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

		#region method Run

		private static void Run(GoogleAnalytics app) {
			Log.Debug("Program.Run started...");

			var oRawByCountry = app.FetchByCountry(m_oReportDate, m_oReportDate);
			var oRawByPage = app.FetchByPage();

			var oStats = new SortedDictionary<string, int>();

			app.ProcessByCountry(oRawByCountry, oStats);

			app.ProcessByPage(oRawByPage, oStats);

			SaveStats(oStats);

			var sourceStats = app.FetchBySource();

			SaveStats(sourceStats);
			Log.Debug("Program.Run complete.");
		} // Run

		#endregion method Run

		#region method Done

		private static void Done() {
			Log.Debug("Program.Done started...");
			Log.Debug("Program.Done complete.");
		} // Done

		#endregion method Done

		#region method SaveStats

		private static void SaveStats(SortedDictionary<string, int> oStats) {
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

		private static void SaveStats(List<StatsModel> oStats)
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

		#region property Environment

		private static Ezbob.Context.Environment Environment { get; set; }

		#endregion property Environment

		#region property Log

		private static ASafeLog Log { get; set; }

		#endregion property Log

		private static DateTime m_oReportDate;
	} // class Program
} // namespace EzAnalyticsConsoleClient
