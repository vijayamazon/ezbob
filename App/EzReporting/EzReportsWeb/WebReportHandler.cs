using System;
using System.Collections.Generic;
using System.Linq;
using Ezbob.Logger;
using Html;
using Html.Tags;
using OfficeOpenXml;
using Reports;
using Ezbob.Database;

namespace EzReportsWeb
{
	using System.Data;
	using Reports.TraficReport;

	public class WebReportHandler : BaseReportHandler
	{
		public WebReportHandler(AConnection oDB, ASafeLog log = null)
			: base(oDB, log)
		{
			ReportList = Report.GetUserReportsList(oDB, System.Web.HttpContext.Current.User.Identity.Name);
		} // constructor

		internal ATag GetReportData(string selectedReport, ReportQuery rptDef, bool isDaily, List<string> oColumnTypes, out bool isError)
		{
			Report report = GetReport(selectedReport);
			isError = false;
			if (report == null)
			{
				isError = true;
				return new Span().Append(new Text("Ops something went wrong, retry"));
			}

			rptDef.Report = report;
			rptDef.StoredProcedure = report.StoredProcedure;
			rptDef.Columns = report.Columns;

			report.IsDaily = isDaily;

			switch (report.Type)
			{
				case ReportType.RPT_PLANNED_PAYTMENT:
					return BuildPlainedPaymentReport(report, (DateTime)rptDef.DateStart, DateTime.UtcNow);

				case ReportType.RPT_EARNED_INTEREST:
					return BuildEarnedInterestReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				case ReportType.RPT_EARNED_INTEREST_ALL_CUSTOMERS:
					return BuildEarnedInterestAllCustomersReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				case ReportType.RPT_FINANCIAL_STATS:
					return BuildFinancialStatsReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				case ReportType.RPT_LOAN_INTEGRITY:
					return BuildLoanIntegrityReport(report, oColumnTypes);

				case ReportType.RPT_LOANS_GIVEN:
					return BuildLoansIssuedReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				case ReportType.RPT_CCI:
					return BuildCciReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				case ReportType.RPT_UI_REPORT:
					return BuildUiReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				case ReportType.RPT_UI_EXT_REPORT:
					return BuildUiExtReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				case ReportType.RPT_ACCOUNTING_LOAN_BALANCE:
					return BuildAccountingLoanBalanceReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				case ReportType.RPT_TRAFFIC_REPORT:
					return BuildTrafficReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);
				
				case ReportType.RPT_MARKETING_CHANNELS_SUMMARY:
					return BuildMarketingChannelsSummaryReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

				default:
					string sReportTitle = report.GetTitle((DateTime)rptDef.DateStart, " ", report.IsDaily ? (DateTime?)null : (DateTime)rptDef.DateEnd);

					return new Div()
						.Append(new H1().Append(new Text(sReportTitle)))
						.Append(TableReport(rptDef, oColumnTypes: oColumnTypes));
			} // switch
		} // GetReportData

		internal ExcelPackage GetWorkBook(System.Web.UI.WebControls.ListItem selectedReport, ReportQuery rptDef, bool isDaily)
		{
			Report report = GetReport(selectedReport.Text);

			if (report == null)
				return ErrorXlsReport("Type reports for this customer cannot be obtained !!!");

			rptDef.Report = report;
			rptDef.StoredProcedure = report.StoredProcedure;

			report.IsDaily = isDaily;

			switch (report.Type)
			{
				case ReportType.RPT_PLANNED_PAYTMENT:
					return BuildPlainedPaymentXls(report, (DateTime)rptDef.DateStart, DateTime.UtcNow);

				case ReportType.RPT_EARNED_INTEREST:
					return BuildEarnedInterestXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_EARNED_INTEREST_ALL_CUSTOMERS:
					return BuildEarnedInterestAllCustomersXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_FINANCIAL_STATS:
					return BuildFinancialStatsXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_LOANS_GIVEN:
					return BuildLoansIssuedXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_CCI:
					return BuildCciXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_UI_REPORT:
					return BuildUiXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_UI_EXT_REPORT:
					return BuildUiExtXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_ACCOUNTING_LOAN_BALANCE:
					return BuildAccountingLoanBalanceXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_TRAFFIC_REPORT:
					return BuildTrafficReportXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				case ReportType.RPT_MARKETING_CHANNELS_SUMMARY:
					return BuildMarketingChannelsSummaryXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

				default:
					var xlsTitle = report.GetTitle((DateTime)rptDef.DateStart, " ", report.IsDaily ? (DateTime?)null : (DateTime)rptDef.DateEnd);
					return XlsReport(rptDef, xlsTitle);
			} // switch
		} // GetWorkBook

		public Report GetReport(string title)
		{
			return ReportList.Values.FirstOrDefault(report => report.Title.Equals(title));
		} // GetReport

		public SortedDictionary<string, Report> ReportList { get; private set; }
	} // class WebReportHandler
} // EzReportsWeb

