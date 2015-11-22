namespace Reports {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {
		protected ATag GetHtml(ReportQuery rptDef, List<string> oColumnTypes, Report report) {
			switch (report.Type) {
			case ReportType.RPT_NOT_AUTO_APPROVED:
				return BuildNotAutoApprovedReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

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

			case ReportType.RPT_STRATEGY_RUNNING_TIME:
				return BuildStrategyRunningTimeReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

			default:
				string sReportTitle = report.GetTitle((DateTime)rptDef.DateStart, " ", report.IsDaily ? (DateTime?)null : (DateTime)rptDef.DateEnd);

				return new Div()
					.Append(new H1().Append(new Text(sReportTitle)))
					.Append(TableReport(rptDef, oColumnTypes: oColumnTypes));
			} // switch
		} // GetHtml

		protected ExcelPackage GetXls(ReportQuery rptDef, Report report) {
			switch (report.Type) {
			case ReportType.RPT_NOT_AUTO_APPROVED:
				return BuildNotAutoApprovedXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

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

			case ReportType.RPT_STRATEGY_RUNNING_TIME:
				return BuildStrategyRunningTimeXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

			default:
				var xlsTitle = report.GetTitle((DateTime)rptDef.DateStart, " ", report.IsDaily ? (DateTime?)null : (DateTime)rptDef.DateEnd);
				return XlsReport(rptDef, xlsTitle);
			} // switch
		} // GetXls
	} // class BaseReportHandler
} // namespace Reports
