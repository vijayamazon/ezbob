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

			var sender = new BaseReportSender(this);

			Parallel.ForEach<Report>(reportList, (report) => {
				Debug(report.Title);

				switch (report.Type) {
				case ReportType.RPT_NEW_CLIENT:
					sender.Send(
						report.Title,
						BuildNewClientReport(report, dToday),
						report.ToEmail,
						"Daily",
						BuildNewClientXls(report, dToday)
					);
					break;

				case ReportType.RPT_PLANNED_PAYTMENT:
					sender.Send(
						report.Title,
						BuildPlainedPaymentReport(report, dToday),
						report.ToEmail,
						"Daily",
						BuildPlainedPaymentXls(report, dToday)
					);
					break;

				case ReportType.RPT_DAILY_STATS:
					sender.Send(
						report.Title,
						BuildDailyStatsReportBody(report, dToday, dTomorrow),
						report.ToEmail,
						"Daily",
						BuildDailyStatsXls(report, dToday, dTomorrow)
					);
					break;

				case ReportType.RPT_IN_WIZARD:
					sender.Send(
						report.Title,
						BuildInWizardReport(report, dToday, dTomorrow),
						report.ToEmail,
						"Daily",
						BuildInWizardXls(report, dToday, dTomorrow)
					);
					break;

				default:
					HandleGenericReport(report, dToday, sender);
					break;
				} // switch
			}); // foreach
		} // ExecuteReportHandler

		private void HandleGenericReport(Report report, DateTime dToday, BaseReportSender sender) {
			if (report.IsDaily)
				BuildReport(report, dToday, dToday.AddDays(1), DailyPerdiod, sender);

			if (IsWeekly(report.IsWeekly, dToday))
				BuildReport(report, dToday.AddDays(-7), dToday, WeeklyPerdiod, sender);

			if (IsMonthly(report.IsMonthly, dToday))
				BuildReport(report, dToday.AddMonths(-1), dToday, MonthlyPerdiod, sender);

			if (report.IsMonthToDate) {
				DateTime monthStart = (new DateTime(dToday.Year, dToday.Month, 1));
				BuildReport(report, monthStart, dToday.AddDays(1), MonthToDatePerdiod, sender);
			} // if month to date
		} // HandleGenericReport

		private void BuildReport(Report report, DateTime fromDate, DateTime toDate, string period, BaseReportSender sender) {
			BaseReportSender.MailTemplate email = sender.CreateMailTemplate();

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

			email.ReportBody.Append(TableReport(report.StoredProcedure, fromDate, toDate, report.Columns, false, email.Title.ToString()));

			sender.Send(
				report.Title,
				email.HtmlBody,
				report.ToEmail,
				period,
				XlsReport(report.StoredProcedure, fromDate, toDate, email.Title.ToString())
			);
		} // BuildReport

		private bool IsMonthly(bool isMonthlyFlag, DateTime dToday) {
			return isMonthlyFlag && dToday.Day == 1;
		} // IsMonthly

		private bool IsWeekly(bool isWeeklyFlag, DateTime dToday) {
			return isWeeklyFlag && dToday.DayOfWeek == DayOfWeek.Sunday;
		} // IsWeekly

		private IEnumerable<Report> GetReportsList() {
			DataTable dt = Report.LoadReportList(DB);

			var reportList = new List<Report>();

			foreach (DataRow row in dt.Rows)
				AddReportToList(reportList, row, BaseReportSender.DefaultToEMail);

			return reportList;
		} // GetReportList
	} // class EmailReportHandler
} // namespace EzReportToEMail
