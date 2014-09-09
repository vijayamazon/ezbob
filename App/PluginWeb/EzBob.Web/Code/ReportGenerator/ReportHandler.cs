namespace EzBob.Web.Code.ReportGenerator
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Html;
	using Html.Tags;
	using OfficeOpenXml;
	using Reports;
	using Reports.TraficReport;

	public class ReportHandler : BaseReportHandler
	{
		public ReportHandler(AConnection oDB, ASafeLog log = null) : base(oDB, log)
		{
		}

		internal ATag GetReportData(Report report, ReportQuery rptDef,  List<string> oColumnTypes, out bool isError)
		{
			isError = false;
			
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
					var trafficReport = new TrafficReport(DB);
					KeyValuePair<ReportQuery, DataTable> oData = trafficReport.CreateTrafficReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);
					return BuildTrafficReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oData, oColumnTypes);

				default:
					string sReportTitle = report.GetTitle((DateTime)rptDef.DateStart, " ", report.IsDaily ? (DateTime?)null : (DateTime)rptDef.DateEnd);

					return new Div()
						.Append(new H1().Append(new Text(sReportTitle)))
						.Append(TableReport(rptDef, oColumnTypes: oColumnTypes));
			} // switch
		} // GetReportData

		internal ExcelPackage GetWorkBook(Report report, ReportQuery rptDef)
		{
			if (report == null)
				return ErrorXlsReport("Type reports for this customer cannot be obtained !!!");

			rptDef.Report = report;
			rptDef.StoredProcedure = report.StoredProcedure;

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

				default:
					var xlsTitle = report.GetTitle((DateTime)rptDef.DateStart, " ", report.IsDaily ? (DateTime?)null : (DateTime)rptDef.DateEnd);
					return XlsReport(rptDef, xlsTitle);
			} // switch
		} // GetWorkBook
	}
}