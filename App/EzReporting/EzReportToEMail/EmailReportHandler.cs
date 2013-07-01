using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using Html.Tags;
using Html.Attributes;
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

		public void ExecuteReportHandler(DateTime dToday) {
			IEnumerable<Report> reportList = GetReportsList();

			DateTime dTomorrow = dToday.AddDays(1);

			Parallel.ForEach<Report>(reportList, (report) => {
				Debug(report.Title);

				switch (report.Type) {
				case ReportType.RPT_NEW_CLIENT:
					SendReport(
						report.Title,
						BuildNewClientReport(report, dToday),
						report.ToEmail,
						"Daily",
						BuildNewClientXls(report, dToday)
					);
					break;

				case ReportType.RPT_PLANNED_PAYTMENT:
					SendReport(
						report.Title,
						BuildPlainedPaymentReport(report, dToday),
						report.ToEmail,
						"Daily",
						BuildPlainedPaymentXls(report, dToday)
					);
					break;

				case ReportType.RPT_DAILY_STATS:
					SendReport(
						report.Title,
						BuildDailyStatsReportBody(report, dToday, dTomorrow),
						report.ToEmail,
						"Daily",
						BuildDailyStatsXls(report, dToday, dTomorrow)
					);
					break;

				case ReportType.RPT_IN_WIZARD:
					SendReport(
						report.Title,
						BuildInWizardReport(report, dToday, dTomorrow),
						report.ToEmail,
						"Daily",
						BuildInWizardXls(report, dToday, dTomorrow)
					);
					break;

				default:
					HandleGenericReport(report, dToday);
					break;
				} // switch
			}); // foreach
		} // ExecuteReportHandler

		private void HandleGenericReport(Report report, DateTime dToday) {
			if (report.IsDaily)
				BuildReport(report, dToday, dToday.AddDays(1), DailyPerdiod);

			if (IsWeekly(report.IsWeekly, dToday))
				BuildReport(report, dToday.AddDays(-7), dToday, WeeklyPerdiod);

			if (IsMonthly(report.IsMonthly, dToday))
				BuildReport(report, dToday.AddMonths(-1), dToday, MonthlyPerdiod);

			if (report.IsMonthToDate) {
				DateTime monthStart = (new DateTime(dToday.Year, dToday.Month, 1));
				BuildReport(report, monthStart, dToday.AddDays(1), MonthToDatePerdiod);
			} // if month to date
		} // HandleGenericReport

		private void BuildReport(Report report, DateTime fromDate, DateTime toDate, string period) {
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
				.Add<Html.Attributes.Title>("Fast business loans for Ebay and Amazon merchants")
				.Add<Alt>("Fast business loans for Ebay and Amazon merchants")
				.Append(oImgLogo);

			var oTr = new Tr();
			oTbl.Append(oTr);

			oTr.Append(new Td().Append(oLogoLink));

			H1 oRptTitle = new H1();

			oTr.Append(new Td().Append(oRptTitle));

			switch (period) {
			case DailyPerdiod:
				oRptTitle.Append(new Text(period + " " + report.GetTitle(fromDate, " for ")));
				break;

			case WeeklyPerdiod:
				oRptTitle.Append(new Text(period + " " + report.GetTitle(fromDate, " for ", toDate)));
				break;

			case MonthlyPerdiod:
				oRptTitle.Append(new Text(period + " " + report.GetMonthTitle(fromDate)));
				break;

			case MonthToDatePerdiod:
				oRptTitle.Append(new Text(period + " " + report.GetMonthTitle(fromDate, toDate)));
				break;
			} // switch

			body.Append(new P().Add<Class>("Body").Append(TableReport(report.StoredProcedure, fromDate, toDate, report.Columns, false, oRptTitle.ToString())));

			SendReport(
				report.Title, 
				body, 
				report.ToEmail, 
				period,
				XlsReport(report.StoredProcedure, fromDate, toDate, oRptTitle.ToString())
			);
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
