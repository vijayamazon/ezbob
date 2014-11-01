namespace EzAnalyticsConsoleClient
{
	using Ezbob.Context;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using EzEnv = Ezbob.Context.Environment;
	using GoogleAnalyticsLib;

	class Program
	{
		static void Main(string[] args)
		{
			Log = new LegacyLog();
			var app = new GoogleAnalytics(Log);
			Environment = new EzEnv(Log);
			string thumb = System.Configuration.ConfigurationManager.AppSettings["gaCertThumb"];
			m_oReportDate = DateTime.Today.AddDays(-1);
			if (Environment.Name == Name.Dev)
			{
				Log = new ConsoleLog();

				if ((args.Length > 1) && (args[0] == "--date"))
					DateTime.TryParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out m_oReportDate);

				if (app.Init(m_oReportDate, thumb))
				{
					Run(app);
				}

				Done();
			}
			else
			{
				try
				{
					if (app.Init(m_oReportDate, thumb))
					{
						if ((args.Length > 1) && (args[0] == "--backfill"))
						{
							RunBackfill(app, args[1], args[2]);
						}
						else
						{
							Run(app);
						}
					}

					Done();
				}
				catch (Exception ex)
				{
					app.Log.Error("Error Occured!\n\nStatement: {0}\n\nDescription: {1}", ex.Message, ex.ToString());
					app.Log.Error("\nPress enter to exit");
				} // try
			} // if dev/non-dev env
		}

		private static void RunBackfill(GoogleAnalytics ga, string dateFrom, string dateTo)
		{
			DateTime from;
			DateTime to;
			DateTime.TryParseExact(dateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out from);
			DateTime.TryParseExact(dateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out to);
			while (from < to)
			{
				ga.SetDate(from);
				m_oReportDate = from;
				Run(ga);
				from = from.AddDays(1);
			}
		}

		private static void Run(GoogleAnalytics app)
		{
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

		private static void Done()
		{
			Log.Debug("Program.Done complete.");
		} // Done

		private static void SaveStats(SortedDictionary<string, int> oStats)
		{
			Log.Debug("Saving stats started...");

			var conn = new SqlConnection(Log);

			string dbDate = conn.DateToString(m_oReportDate);

			foreach (KeyValuePair<string, int> pair in oStats)
			{
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
					new QueryParameter(DbConsts.IsaspCodeName, stat.CodeName),
					new QueryParameter(DbConsts.IsaspValue, stat.Value),
					new QueryParameter(DbConsts.IsaspSource, stat.Source)
				);
			} // for each stat to save

			Log.Debug("Saving stats complete.");
		}

		private static Ezbob.Context.Environment Environment { get; set; }

		private static ASafeLog Log { get; set; }

		private static DateTime m_oReportDate;
	} // class Program
} // namespace EzAnalyticsConsoleClient
