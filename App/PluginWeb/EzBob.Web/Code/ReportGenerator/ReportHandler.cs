namespace EzBob.Web.Code.ReportGenerator
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Html;
	using Html.Tags;
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
					return BuildPlainedPaymentReport(report, (DateTime)rptDef.DateStart);

				case ReportType.RPT_EARNED_INTEREST:
					return BuildEarnedInterestReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd, oColumnTypes);

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
	}
}