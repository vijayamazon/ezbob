namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using GoogleAnalyticsLib;

	public class UpdateGoogleAnalytics : AStrategy {

		public UpdateGoogleAnalytics(DateTime? oBackfillStartDate, DateTime? oBackfillEndDate, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			if ((oBackfillStartDate != null) && (oBackfillEndDate != null)) {
				if (oBackfillStartDate.Value <= oBackfillEndDate.Value) {
					m_oBackfillStartDate = oBackfillStartDate;
					m_oBackfillEndDate = oBackfillEndDate;
				}
				else {
					m_oBackfillStartDate = oBackfillEndDate;
					m_oBackfillEndDate = oBackfillStartDate;
				} // if
			} // if

			m_oReportDate = DateTime.Today.AddDays(-1);

			m_oApp = new GoogleAnalytics(Log);

			m_sCertThumb = CurrentValues.Instance.GoogleAnalyticsCertThumb;
		} // constructor

		public override string Name {
			get { return "UpdateGoogleAnalytics"; }
		} // Name

		public override void Execute() {
			try {
				Log.Debug("Updating Google analytics data...");

				if (m_oApp.Init(m_oReportDate, m_sCertThumb)) {
					if (m_oBackfillStartDate.HasValue && m_oBackfillEndDate.HasValue) {
						DateTime cur = m_oBackfillStartDate.Value;

						while (cur < m_oBackfillEndDate.Value) {
							m_oReportDate = cur;
							m_oApp.SetDate(cur);
							UpdateCurrentReportDate();
							cur = cur.AddDays(1);
						} // while
					}
					else
						UpdateCurrentReportDate();
				} // if

				Log.Debug("Updating Google analytics data complete.");
			}
			catch (Exception ex) {
				Log.Alert(ex, "Error updating Google analytics data.");
			} // try
		} // Execute

		private readonly GoogleAnalytics m_oApp;
		private readonly DateTime? m_oBackfillStartDate;
		private readonly DateTime? m_oBackfillEndDate;
		private readonly string m_sCertThumb;
		private DateTime m_oReportDate;

		private string ReportDateStr {
			get { return m_oReportDate.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture); }
		} // ReportDateStr

		private void UpdateCurrentReportDate() {
			Log.Debug("UpdateCurrentReportDate('{0}') started...", ReportDateStr);

			SortedDictionary<string, CountryData> oRawByCountry = m_oApp.FetchByCountry(m_oReportDate, m_oReportDate);
			SortedDictionary<PageID, int> oRawByPage = m_oApp.FetchByPage();

			var oStats = new SortedDictionary<string, int>();

			m_oApp.ProcessByCountry(oRawByCountry, oStats);

			m_oApp.ProcessByPage(oRawByPage, oStats);

			SaveStats(oStats, "by country and by page");

			SaveStats(m_oApp.FetchBySource(), "by source");

			SaveStats(m_oApp.FetchByLandingPage(), "by landing page");

			Log.Debug("UpdateCurrentReportDate('{0}') complete.", ReportDateStr);
		} // UpdateCurrentReportDate

		private void SaveStats(SortedDictionary<string, int> oStats, string sDescription) {
			Log.Debug("Saving {1} stats for {0} started...", ReportDateStr, sDescription);

			var sp = new InsertOrUpdateSiteAnalytics(DB, Log);

			foreach (KeyValuePair<string, int> pair in oStats) {
				var oRow = new InsertOrUpdateSiteAnalytics.InputRow(m_oReportDate, pair.Key, pair.Value);

				if (oRow.IsValid(Log, Severity.Debug))
					sp.lst.Add(oRow);
			} // for each stat to save

			if (sp.AreParametersValid(Severity.Debug))
				sp.ExecuteNonQuery();

			Log.Debug("Saving {1} stats for {0} complete.", ReportDateStr, sDescription);
		} // SaveStats

		private void SaveStats(IEnumerable<StatsModel> oStats, string sDescription) {
			Log.Debug("Saving {1} stats for {0} started...", ReportDateStr, sDescription);

			var sp = new InsertOrUpdateSiteAnalytics(DB, Log);

			foreach (StatsModel stat in oStats) {
				var oRow = new InsertOrUpdateSiteAnalytics.InputRow(m_oReportDate, stat);

				if (oRow.IsValid(Log, Severity.Debug))
					sp.lst.Add(oRow);
			} // for each stat to save

			if (sp.AreParametersValid(Severity.Debug))
				sp.ExecuteNonQuery();

			Log.Debug("Saving {1} stats for {0} complete.", ReportDateStr, sDescription);
		} // SaveStats

	} // class UpdateGoogleAnalytics
} // namespace
