using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Html.Tags;
using Reports;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzReportToEMail {
	using System.Data;
	using Html;
	using Html.Attributes;
	using OfficeOpenXml;
	using Reports.TraficReport;

	#region class EmailReportHandler

	public class EmailReportHandler : BaseReportHandler {
		#region public

		#region constructor

		public EmailReportHandler(AConnection oDB, ASafeLog log = null)
			: base(oDB, log) {
		} // constructor

		#endregion constructor

		#region method ExecuteReportHandler

		public void ExecuteReportHandler(DateTime dToday, ReportType? nReportTypeToExecute) {
			SortedDictionary<string, Report> reportList = Report.GetScheduledReportsList(DB);

			var sender = new ReportDispatcher(DB, this);
			try {
				Parallel.ForEach(reportList.Values, report => {
					try {
						bool bExecute =
							(nReportTypeToExecute == null) ||
							(report.Type == nReportTypeToExecute.Value);

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
							var trafficReport = new TrafficReport(DB, this);
							DateTime dYesterday = dToday.AddDays(-1);

							if (report.IsDaily) {
								sender.Dispatch(
									report.Title,
									dYesterday,
									BuildTrafficReport(report, dYesterday, dToday),
									BuildTrafficReportXls(report, dYesterday, dToday),
									report.ToEmail
								);
							}

							if (report.IsMonthToDate) {
								var dFirstOfMonth = new DateTime(dToday.Year, dToday.Month, 1);

								sender.Dispatch(
									report.Title,
									dYesterday,
									BuildTrafficReport(report, dFirstOfMonth, dToday),
									BuildTrafficReportXls(report, dFirstOfMonth, dToday),
									report.ToEmail
								);
							}
							break;

						case ReportType.RPT_PLANNED_PAYTMENT:
							HandleGenericReport(report, dToday, sender, BuildPlainedPaymentReport, BuildPlainedPaymentXls);
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

						default:
							HandleGenericReport(report, dToday, sender, null, null);
							break;
						} // switch

						Debug("Generating {0} report complete.", report.Title);
					}
					catch (Exception ex) {
						Error("Generating {0} report failed \n {1}.", report.Title, ex);

						sender.Dispatch("Report to mail tool: error generating report",
							DateTime.UtcNow,
							new Html.Tags.Body()
								.Add<Class>("Body")
								.Append(new H1()
								.Append(new Text(string.Format("Error Generating/Sending report {0}", report.Title)))),
							null,
							"operations@ezbob.com"
						);
					}
				}); // foreach
			}
			catch (AggregateException ae) {
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
			}
		} // ExecuteReportHandler

		#endregion method ExecuteReportHandler

		#endregion public

		#region private

		#region method HandleGenericReport

		private void HandleGenericReport(
			Report report,
			DateTime dToday,
			ReportDispatcher sender,
			Func<Report, DateTime, DateTime, List<string>, ATag> oBuildHtml,
			Func<Report, DateTime, DateTime, ExcelPackage> oBuildXls
		) {
			if (report.IsDaily)
				BuildReport(report, dToday, dToday.AddDays(1), DailyPerdiod, sender, dToday, oBuildHtml, oBuildXls);

			if (IsWeekly(report.IsWeekly, dToday))
				BuildReport(report, dToday.AddDays(-7), dToday, WeeklyPerdiod, sender, dToday, oBuildHtml, oBuildXls);

			if (IsMonthly(report.IsMonthly, dToday))
				BuildReport(report, dToday.AddMonths(-1), dToday, MonthlyPerdiod, sender, dToday, oBuildHtml, oBuildXls);

			if (report.IsMonthToDate) {
				var monthStart = new DateTime(dToday.Year, dToday.Month, 1);
				BuildReport(report, monthStart, dToday.AddDays(1), MonthToDatePerdiod, sender, dToday, oBuildHtml, oBuildXls);
			} // if month to date
		} // HandleGenericReport

		#endregion method HandleGenericReport

		#region method BuildReport

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

		#endregion method BuildReport

		#region method IsMonthly

		private bool IsMonthly(bool isMonthlyFlag, DateTime dToday) {
			return isMonthlyFlag && dToday.Day == 1;
		} // IsMonthly

		#endregion method IsMonthly

		#region method IsWeekly

		private bool IsWeekly(bool isWeeklyFlag, DateTime dToday) {
			return isWeeklyFlag && dToday.DayOfWeek == DayOfWeek.Sunday;
		} // IsWeekly

		#endregion method IsWeekly

		#region const

		private const string DailyPerdiod = "Daily";
		private const string WeeklyPerdiod = "Weekly";
		private const string MonthlyPerdiod = "Monthly";
		private const string MonthToDatePerdiod = "Month to Date";

		#endregion const

		#endregion private
	} // class EmailReportHandler

	#endregion class EmailReportHandler
} // namespace EzReportToEMail
