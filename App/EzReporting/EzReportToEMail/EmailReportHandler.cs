using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using Html;
using Reports;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzReportToEMail {
	public class EmailReportHandler : BaseReportHandler {
		private const string DailyPerdiod = "Daily";
		private const string WeeklyPerdiod = "Weekly";
		private const string MonthlyPerdiod = "Monthly";
		private const string MonthToDatePerdiod = "Month to Date";

		public EmailReportHandler(AConnection oDB, ASafeLog log = null) : base(oDB, log) {
		} // constructor

		public void ExecuteReportHandler() {
			IEnumerable<Report> reportList = GetReportsList();

			var dToday = DateTime.Today;

			string today = dToday.ToString("yyyy-MM-dd");
			string tomorrow = dToday.AddDays(1).ToString("yyyy-MM-dd");

			Parallel.ForEach<Report> (reportList, (report) => {
				Debug(report.Title);

				switch (report.Type) {
				case ReportType.RPT_NEW_CLIENT:
					SendReport(report.Title, BuildNewClientReport(report, today, tomorrow), report.ToEmail);
					break;

				case ReportType.RPT_PLANNED_PAYTMENT:
					SendReport(report.Title, BuildPlainedPaymentReport(report, today, tomorrow), report.ToEmail);
					break;

				case ReportType.RPT_DAILY_STATS:
					SendReport(report.Title, BuildDailyStatsReportBody(report, today, tomorrow), report.ToEmail);
					break;

				case ReportType.RPT_IN_WIZARD:
					SendReport(report.Title, BuildInWizardReport(report, today, tomorrow), report.ToEmail);
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
			var body = new Body().Add<Class>("Body");

			var oTbl = new Table().Add<Class>("Header");
			body.Append(oTbl);

			var oImgLogo = new Img()
				.Add<Class>("Logo")
				.Add<Src>("http://www.ezbob.com/wp-content/themes/ezbob/images/ezbob_logo.png");

			var oLogoLink = new A()
				.Add<Href>("http://www.ezbob.com/")
				.Add<Class>("logo_ezbob")
				.Add<Class>("indent_text")
				.Add<ID>("ezbob_logo")
				.Add<Title>("Fast business loans for Ebay and Amazon merchants")
				.Add<Alt>("Fast business loans for Ebay and Amazon merchants")
				.Append(oImgLogo);

			var oTr = new Tr();
			oTbl.Append(oTr);

			oTr.Append(new Td().Append(oLogoLink));

			H1 oRptTitle = new H1();

			oTr.Append(new Td().Append(oRptTitle));

			string month = DateTime.Parse(fromDate).ToString("MMMMM yyyy", CultureInfo.GetCultureInfo("en-GB"));

			switch (period) {
			case DailyPerdiod:
				oRptTitle.Append(new Text(period + " " + report.Title + " for " + fromDate));
				break;

			case WeeklyPerdiod:
				oRptTitle.Append(new Text(period + " " + report.Title + " for " + fromDate + " - " + toDate));
				break;

			case MonthlyPerdiod:
				oRptTitle.Append(new Text(period + " " + report.Title + " for " + month));
				break;

			case MonthToDatePerdiod:
				oRptTitle.Append(new Text(period + " " + report.Title + " for " + month + " Until " + toDate));
				break;
			} // switch

			body.Append(new P().Add<Class>("Body").Append(TableReport(report.StoredProcedure, fromDate, toDate, report.Columns)));

			SendReport(report.Title, body, report.ToEmail, period);
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
