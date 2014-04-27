﻿using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Html.Tags;
using Reports;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzReportToEMail
{
	using System.Data;
	using Html.Attributes;
	using Reports.TraficReport;

	#region class EmailReportHandler

	public class EmailReportHandler : BaseReportHandler
	{
		#region public

		#region constructor

		public EmailReportHandler(AConnection oDB, ASafeLog log = null)
			: base(oDB, log)
		{
		} // constructor

		#endregion constructor

		#region method ExecuteReportHandler

		public void ExecuteReportHandler(DateTime dToday)
		{
			SortedDictionary<string, Report> reportList = Report.GetScheduledReportsList(DB);

			DateTime dTomorrow = dToday.AddDays(1);
			DateTime dYesterday = dToday.AddDays(-1);

			var sender = new ReportDispatcher(DB, this);
			try
			{
				Parallel.ForEach(reportList.Values, report =>
					{
						try
						{
							Debug("Generating {0} report...", report.Title);

							switch (report.Type)
							{
								case ReportType.RPT_PLANNED_PAYTMENT:
									sender.Dispatch(
										report.Title,
										dToday,
										BuildPlainedPaymentReport(report, dToday),
										BuildPlainedPaymentXls(report, dToday),
										report.ToEmail
										);
									break;

								case ReportType.RPT_EARNED_INTEREST:
									sender.Dispatch(
										report.Title,
										dToday,
										BuildEarnedInterestReport(report, dToday, dTomorrow),
										BuildEarnedInterestXls(report, dToday, dTomorrow),
										report.ToEmail
										);
									break;

								case ReportType.RPT_LOANS_GIVEN:
									sender.Dispatch(
										report.Title,
										dToday,
										BuildLoansIssuedReport(report, dToday, dTomorrow),
										BuildLoansIssuedXls(report, dToday, dTomorrow),
										report.ToEmail
										);
									break;

								case ReportType.RPT_LOAN_STATS:
									sender.Dispatch(
										"loan_stats",
										dToday,
										null,
										new LoanStats(DB, this).Xls(),
										ReportDispatcher.ToDropbox
										);
									break;

								case ReportType.RPT_CCI:
									sender.Dispatch(
										report.Title,
										dToday,
										BuildCciReport(report, dToday, dTomorrow),
										BuildCciXls(report, dToday, dTomorrow),
										report.ToEmail
										);
									break;

								case ReportType.RPT_UI_REPORT:
									sender.Dispatch(
										report.Title,
										dToday,
										BuildUiReport(report, dToday, dTomorrow),
										BuildUiXls(report, dToday, dTomorrow),
										report.ToEmail
										);
									break;

								case ReportType.RPT_UI_EXT_REPORT:
									sender.Dispatch(
										report.Title,
										dToday,
										BuildUiExtReport(report, dToday, dTomorrow),
										BuildUiExtXls(report, dToday, dTomorrow),
										report.ToEmail
										);
									break;

								case ReportType.RPT_ACCOUNTING_LOAN_BALANCE:
									sender.Dispatch(
										report.Title,
										dToday,
										BuildAccountingLoanBalanceReport(report, dToday, dTomorrow),
										BuildAccountingLoanBalanceXls(report, dToday, dTomorrow),
										report.ToEmail
										);
									break;

								case ReportType.RPT_TRAFFIC_REPORT:
									var trafficReport = new TrafficReport(DB);
									if (report.IsDaily)
									{
										KeyValuePair<ReportQuery, DataTable> oData = trafficReport.CreateTrafficReport(report, dYesterday, dToday);

										sender.Dispatch(
											report.Title,
											dYesterday,
											BuildTrafficReport(report, dYesterday, dToday, oData),
											BuildTrafficReportXls(report, dYesterday, dToday, oData),
											report.ToEmail);
									}
									if (report.IsMonthToDate)
									{
										var dFirstOfMonth = new DateTime(dToday.Year, dToday.Month, 1);
										KeyValuePair<ReportQuery, DataTable> oData = trafficReport.CreateTrafficReport(report, dFirstOfMonth, dToday);

										sender.Dispatch(
											report.Title,
											dYesterday,
											BuildTrafficReport(report, dFirstOfMonth, dToday, oData),
											BuildTrafficReportXls(report, dFirstOfMonth, dToday, oData),
											report.ToEmail);
									}
									break;

								default:
									HandleGenericReport(report, dToday, sender);
									break;
							} // switch

							Debug("Generating {0} report complete.", report.Title);
						}
						catch (Exception ex)
						{
							Error("Generating {0} report failed \n {1}.", report.Title, ex);
							sender.Dispatch("Report to mail tool: error generating report",
								DateTime.UtcNow,
								new Html.Tags.Body()
								.Add<Class>("Body")
								.Append(new H1()
								.Append(new Text(string.Format("Error Generating/Sending report {0}", report.Title)))),
								null,
								"operations@ezbob.com");
						}
					}); // foreach
			}
			catch (AggregateException ae)
			{
				ae.Handle(x =>
				{
					if (x is ArgumentNullException)
					{
						Error("Parallel Exception ArgumentNullException {0}", x);
						// manage the exception.
						return true; // do not stop the program
					}
					else if (x is UnauthorizedAccessException)
					{
						Error("Parallel Exception UnauthorizedAccessException {0}", x);
						// manage the access error.
						return true;
					}
					else //is (Exception ex)
					{
						Error("Parallel Exception {0}", x);
						// Any other exception here
					}
					return false; // Let anything else stop the application.
				});
			}
		} // ExecuteReportHandler

		#endregion method ExecuteReportHandler

		#endregion public

		#region private

		#region method HandleGenericReport

		private void HandleGenericReport(Report report, DateTime dToday, ReportDispatcher sender)
		{
			if (report.IsDaily)
				BuildReport(report, dToday, dToday.AddDays(1), DailyPerdiod, sender, dToday);

			if (IsWeekly(report.IsWeekly, dToday))
				BuildReport(report, dToday.AddDays(-7), dToday, WeeklyPerdiod, sender, dToday);

			if (IsMonthly(report.IsMonthly, dToday))
				BuildReport(report, dToday.AddMonths(-1), dToday, MonthlyPerdiod, sender, dToday);

			if (report.IsMonthToDate)
			{
				DateTime monthStart = (new DateTime(dToday.Year, dToday.Month, 1));
				BuildReport(report, monthStart, dToday.AddDays(1), MonthToDatePerdiod, sender, dToday);
			} // if month to date
		} // HandleGenericReport

		#endregion method HandleGenericReport

		#region method BuildReport

		private void BuildReport(Report report, DateTime fromDate, DateTime toDate, string period, ReportDispatcher sender, DateTime oReportGenerationDate)
		{
			var email = new ReportEmail();

			switch (period)
			{
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

			email.ReportBody.Append(TableReport(
				rptDef,
				false,
				email.Title.ToString()
			));

			sender.Dispatch(
				report.Title,
				oReportGenerationDate,
				email.HtmlBody,
				XlsReport(rptDef, email.Title.ToString()),
				report.ToEmail,
				period
			);
		} // BuildReport

		#endregion method BuildReport

		#region method IsMonthly

		private bool IsMonthly(bool isMonthlyFlag, DateTime dToday)
		{
			return isMonthlyFlag && dToday.Day == 1;
		} // IsMonthly

		#endregion method IsMonthly

		#region method IsWeekly

		private bool IsWeekly(bool isWeeklyFlag, DateTime dToday)
		{
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
