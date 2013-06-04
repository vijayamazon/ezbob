using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using Reports;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzReportToEMail {
	public class EmailReportHandler : BaseReportHandler {
		private const string DailyPerdiod = "Daily";
		private const string WeeklyPerdiod = "Weekly";
		private const string MonthlyPerdiod = "Monthly";
		private const string MonthToDatePerdiod = "Month to Date";
		private static string Lock = "";

		public EmailReportHandler(AConnection oDB, ASafeLog log = null) : base(oDB, log) {
		} // constructor

		public void ExecuteReportHandler() {
			IEnumerable<Report> reportList = GetReportsList();

			var dToday = DateTime.Today;

			string today = dToday.ToString("yyyy-MM-dd");
			string tomorrow = dToday.AddDays(1).ToString("yyyy-MM-dd");

			Parallel.ForEach<Report> (reportList, (report) => {
				lock (Lock) {
					Debug(report.Title);
				}

				switch (report.Type) {
				case ReportType.RPT_NEW_CLIENT:
					string newClientReportBody = BuildNewClientReport(report, today, tomorrow);
					SendReport(report.Title, newClientReportBody, report.ToEmail);
					break;

				case ReportType.RPT_PLANNED_PAYTMENT:
					string plainedPaymentReportBody = BuildPlainedPaymentReport(report, today, tomorrow);
					SendReport(report.Title, plainedPaymentReportBody, report.ToEmail);
					break;

				case ReportType.RPT_DAILY_STATS:
					string dailyStatsReportBody = BuildDailyStatsReportBody(report, today, tomorrow);
					SendReport(report.Title, dailyStatsReportBody, report.ToEmail);
					break;

				case ReportType.RPT_IN_WIZARD:
					string inWizardReportBody = BuildInWizardReport(report, today, tomorrow);
					SendReport(report.Title, inWizardReportBody, report.ToEmail);
					break;

				default:
					HandleGenericReport(report, dToday);
					break;
				} // switch
			}); // foreach
		} // ExecuteReportHandler

		private void HandleGenericReport(Report report, DateTime dToday) {
			string today = dToday.ToString("yyyy-MM-dd");

			if (report.IsDaily) {
				string tomorrow = dToday.AddDays(1).ToString("yyyy-MM-dd");
				BuildReport(report, today, tomorrow, DailyPerdiod);
			} // if daily

			if (IsWeekly(report.IsWeekly, dToday)) {
				string weekAgo = dToday.AddDays(-7).ToString("yyyy-MM-dd");
				BuildReport(report, weekAgo, today, WeeklyPerdiod);
			} // if weekly

			if (IsMonthly(report.IsMonthly, dToday)) {
				string monthAgo = dToday.AddMonths(-1).ToString("yyyy-MM-dd");
				BuildReport(report, monthAgo, today, MonthlyPerdiod);
			} // if monthly

			if (report.IsMonthToDate) {
				string monthStart = (new DateTime(dToday.Year, dToday.Month, 1)).ToString("yyyy-MM-dd");
				string tomorrow = dToday.AddDays(1).ToString("yyyy-MM-dd");
				BuildReport(report, monthStart, tomorrow, MonthToDatePerdiod);
			} // if month to date
		} // HandleGenericReport

		private void BuildReport(Report report, string fromDate, string toDate, string period) {
			var bodyText = new StringBuilder();
			bodyText.Append(Reports.ReportsStyling.BodyHtmlStyle);
			bodyText.Append(ReportsStyling.HeaderDiv).Append(ReportsStyling.HeaderLogo);

			switch (period) {
			case DailyPerdiod:
				bodyText.Append("<td><h1> " + period + " " + report.Title + " " + fromDate + "</h1></td>");
				break;

			case WeeklyPerdiod:
				bodyText.Append("<td><h1> " + period + " " + report.Title + " " + fromDate + " - " + toDate + "</h1></td>");
				break;

			case MonthlyPerdiod:
				string month = DateTime.Parse(fromDate).ToString("MMMMM", CultureInfo.GetCultureInfo("en-US"));
				bodyText.Append("<td><h1> " + period + " " + report.Title + " " + month + "</h1></td>");
				break;

			case MonthToDatePerdiod:
				bodyText.Append("<td><h1> " + period + " " + report.Title + " until " + toDate + "</h1></td>");
				break;
			} // switch

			bodyText.Append("</tr></table>");
			TableReport(bodyText, report.StoredProcedure, fromDate, toDate, report.Headers, report.Fields);
			bodyText.Append("</body>");

			SendReport(report.Title, bodyText.ToString(), report.ToEmail, period);
		} // BuildReport

		private bool IsMonthly(bool isMonthlyFlag, DateTime dToday) {
			return isMonthlyFlag && dToday.Day == 1;
		} // IsMonthly

		private bool IsWeekly(bool isWeeklyFlag, DateTime dToday) {
			return isWeeklyFlag && dToday.DayOfWeek == DayOfWeek.Sunday;
		} // IsWeekly

		private IEnumerable<Report> GetReportsList() {
			DataTable dt = DB.ExecuteReader("RptScheduler_GetReportList");

			var reportList = new List<Report>();

			foreach (DataRow row in dt.Rows)
				AddReportToList(reportList, row, DefaultToEMail);

			return reportList;
		} // GetReportList
	} // class EmailReportHandler
} // namespace EzReportToEMail
