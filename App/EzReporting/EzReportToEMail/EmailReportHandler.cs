namespace EzReportToEMail {
	using System.Threading.Tasks;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Utils.Html.Tags;
	using Reports;
	using Reports.LoanStats;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using OfficeOpenXml;

	public class EmailReportHandler : BaseReportHandler {
		public EmailReportHandler(AConnection oDB, ASafeLog log = null) : base(oDB, log) {
		} // constructor

		public void ExecuteReportHandler(DateTime dToday, ReportType? nReportTypeToExecute) {
			SortedDictionary<string, Report> reportList = Report.GetScheduledReportsList(DB);

			var sender = new ReportDispatcher(DB, this);
			int reportsToHandleParalely = 10;
			bool handle = true;
			try {
				do {
					if (reportList.Count < reportsToHandleParalely)
						reportsToHandleParalely = reportList.Count;

					if (reportList.Count == 0)
						handle = false;

					if (reportsToHandleParalely > 0)
						HandleReportsBulk(reportList, reportsToHandleParalely, dToday, nReportTypeToExecute, sender);
				} while (handle);
			} catch (AggregateException ae) {
				ae.Handle(x => {
					if (x is ArgumentNullException) {
						Error("Parallel Exception ArgumentNullException {0}", x);
						// manage the exception.
						return true; // do not stop the program
					} // if

					if (x is UnauthorizedAccessException) {
						Error("Parallel Exception UnauthorizedAccessException {0}", x);
						// manage the access error.
						return true;
					} // if

					Error("Parallel Exception {0}", x);

					// Any other exception here
					return false; // Let anything else stop the application.
				});
			} // try
		} // ExecuteReportHandler

		private void HandleReportsBulk(
			SortedDictionary<string, Report> reportList,
			int reportsToHandleParalely,
			DateTime dToday,
			ReportType? nReportTypeToExecute,
			ReportDispatcher sender
		) {
			var reportsToHandle = reportList.Take(reportsToHandleParalely).ToList();

			foreach (var rep in reportsToHandle)
				reportList.Remove(rep.Key);

			Parallel.ForEach(reportsToHandle, report => {
				HandleOneReport(dToday, nReportTypeToExecute, report.Value, sender);
			}); // foreach
		} // HandleReportsBulk

		private void HandleOneReport(
			DateTime dToday,
			ReportType? nReportTypeToExecute,
			Report report,
			ReportDispatcher sender
		) {
			try {
				bool bExecute = (nReportTypeToExecute == null) || (report.Type == nReportTypeToExecute.Value);

				if (!bExecute) {
					Debug(
						"Skipping {0} report: only {1} requested.",
						report.Title,
						nReportTypeToExecute.Value.ToString()
						);
					return;
				} // if

				Debug("Generating {0} report...", report.Title);

				switch (report.Type) {
				case ReportType.RPT_LOAN_STATS:
					sender.Dispatch(
						"loan_stats",
						dToday,
						null,
						new LoanStats(DB, this).Xls(),
						ReportDispatcher.ToDropbox
					);
					break;

				case ReportType.RPT_TRAFFIC_REPORT:
					DateTime dYesterday = dToday.AddDays(-1);
					HandleGenericReport(report, dYesterday, sender, BuildTrafficReport, BuildTrafficReportXls, dToday);
					break;

				case ReportType.RPT_NOT_AUTO_APPROVED:
					HandleGenericReport(report, dToday, sender, BuildNotAutoApprovedReport, BuildNotAutoApprovedXls);
					break;

				case ReportType.RPT_EARNED_INTEREST:
					HandleGenericReport(report, dToday, sender, BuildEarnedInterestReport, BuildEarnedInterestXls);
					break;

				case ReportType.RPT_EARNED_INTEREST_ALL_CUSTOMERS:
					HandleGenericReport(report, dToday, sender, BuildEarnedInterestAllCustomersReport, BuildEarnedInterestAllCustomersXls);
					break;

				case ReportType.RPT_FINANCIAL_STATS:
					HandleGenericReport(report, dToday, sender, BuildFinancialStatsReport, BuildFinancialStatsXls);
					break;

				case ReportType.RPT_LOANS_GIVEN:
					HandleGenericReport(report, dToday, sender, BuildLoansIssuedReport, BuildLoansIssuedXls);
					break;

				case ReportType.RPT_CCI:
					HandleGenericReport(report, dToday, sender, BuildCciReport, BuildCciXls);
					break;

				case ReportType.RPT_UI_REPORT:
					HandleGenericReport(report, dToday, sender, BuildUiReport, BuildUiXls);
					break;

				case ReportType.RPT_UI_EXT_REPORT:
					HandleGenericReport(report, dToday, sender, BuildUiExtReport, BuildUiExtXls);
					break;

				case ReportType.RPT_ACCOUNTING_LOAN_BALANCE:
					HandleGenericReport(report, dToday, sender, BuildAccountingLoanBalanceReport, BuildAccountingLoanBalanceXls);
					break;

				case ReportType.RPT_MARKETING_CHANNELS_SUMMARY:
					HandleGenericReport(report, dToday, sender, BuildMarketingChannelsSummaryReport, BuildMarketingChannelsSummaryXls);
					break;

				case ReportType.RPT_STRATEGY_RUNNING_TIME:
					HandleGenericReport(report, dToday, sender, BuildStrategyRunningTimeReport, BuildStrategyRunningTimeXls);
					break;

				default:
					HandleGenericReport(report, dToday, sender, null, null);
					break;
				} // switch

				Debug("Generating {0} report complete.", report.Title);
			} catch (Exception ex) {
				Error("Generating {0} report failed \n {1}.", report.Title, ex);

				sender.Dispatch("Report to mail tool: error generating report",
					DateTime.UtcNow,
					new Body()
						.Add<Class>("Body")
						.Append(new H1()
							.Append(new Text(string.Format("Error Generating/Sending report {0}", report.Title)))),
					null,
					"operations@ezbob.com"
				);
			} // try
		} // HandleOneReport

		private void HandleGenericReport(
			Report report,
			DateTime dToday,
			ReportDispatcher sender,
			Func<Report, DateTime, DateTime, List<string>, ATag> oBuildHtml,
			Func<Report, DateTime, DateTime, ExcelPackage> oBuildXls,
			DateTime? dTodayForMonthStart = null
		) {
            if (string.IsNullOrEmpty(report.ToEmail)) {
                Debug("No emails defined for report {0} {1} to be sent to, not sending", report.Type, report.Title);
                return;
            }

            if (report.IsDaily)
                BuildReport(report, dToday, dToday.AddDays(1), DailyPerdiod, sender, dToday, oBuildHtml, oBuildXls);

			if (IsWeekly(report.IsWeekly, dToday))
				BuildReport(report, dToday.AddDays(-6), dToday.AddDays(1), WeeklyPerdiod, sender, dToday, oBuildHtml, oBuildXls);

            if (IsMonthly(report.IsMonthly, dToday))
                BuildReport(report, dToday.AddMonths(-1), dToday, MonthlyPerdiod, sender, dToday, oBuildHtml, oBuildXls);

            if (report.IsMonthToDate) {
                DateTime d = dTodayForMonthStart.HasValue ? dTodayForMonthStart.Value : dToday;
                var monthStart = new DateTime(d.Year, d.Month, 1);
                BuildReport(report, monthStart, dToday.AddDays(1), MonthToDatePerdiod, sender, dToday, oBuildHtml, oBuildXls);
            } // if month to date
		} // HandleGenericReport

		private void BuildReport(
			Report report,
			DateTime fromDate,
			DateTime toDate,
			string period,
			ReportDispatcher sender,
			DateTime oReportGenerationDate,
			Func<Report, DateTime, DateTime, List<string>, ATag> oBuildHtml,
			Func<Report, DateTime, DateTime, ExcelPackage> oBuildXls
		) {
			Debug("Building report {0} for period {1}", report.Title, period);

			var email = new ReportEmail();

			switch (period) {
			case DailyPerdiod:
				email.Title.Append(new Text(period + " " + report.GetTitle(fromDate, " for ")));
				break;

			case WeeklyPerdiod:
				email.Title.Append(new Text(period + " " + report.GetTitle(fromDate, " for ", toDate)));
				break;

			case MonthlyPerdiod:
				email.Title.Append(new Text(period + " " + report.GetMonthTitle(fromDate)));
				break;

			case MonthToDatePerdiod:
				email.Title.Append(new Text(period + " " + report.GetMonthTitle(fromDate, toDate)));
				break;
			} // switch

			var rptDef = new ReportQuery(report, fromDate, toDate);

			ATag oBody = oBuildHtml == null
				? TableReport(rptDef, false, email.Title.ToString())
				: oBuildHtml(report, fromDate, toDate, null);

			ExcelPackage oXls = oBuildXls == null
				? XlsReport(rptDef, email.Title.ToString())
				: oBuildXls(report, fromDate, toDate);

			email.ReportBody.Append(oBody);

			sender.Dispatch(
				report.Title,
				oReportGenerationDate,
				email.HtmlBody,
				oXls,
				report.ToEmail,
				period
			);
		} // BuildReport

		private bool IsMonthly(bool isMonthlyFlag, DateTime dToday) {
			return isMonthlyFlag && dToday.Day == 1;
		} // IsMonthly

		private bool IsWeekly(bool isWeeklyFlag, DateTime dToday) {
			return isWeeklyFlag && dToday.DayOfWeek == DayOfWeek.Saturday;
		} // IsWeekly

		private const string DailyPerdiod = "Daily";
		private const string WeeklyPerdiod = "Weekly";
		private const string MonthlyPerdiod = "Monthly";
		private const string MonthToDatePerdiod = "Month to Date";
	} // class EmailReportHandler
} // namespace EzReportToEMail
