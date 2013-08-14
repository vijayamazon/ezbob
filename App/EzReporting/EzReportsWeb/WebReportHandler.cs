using System;
using System.Collections.Generic;
using System.Linq;
using Ezbob.Logger;
using Html;
using Html.Tags;
using Reports;
using Ezbob.Database;
using Aspose.Cells;

namespace EzReportsWeb {
	public class WebReportHandler : BaseReportHandler {
		public WebReportHandler(AConnection oDB, ASafeLog log = null) : base(oDB, log) {
			reportList = GetReportsList(System.Web.HttpContext.Current.User.Identity.Name);
		} // constructor

		internal ATag GetReportData(System.Web.UI.WebControls.ListItem selectedReport, ReportQuery rptDef, bool isDaily, List<string> oColumnTypes) {
			Report report = GetReport(selectedReport.Text);

			if (report == null)
				return new Span().Append(new Text("Error Occured, try later"));

			rptDef.Report = report;
			rptDef.StoredProcedure = report.StoredProcedure;
			rptDef.Columns = report.Columns;

			report.IsDaily = isDaily;

			switch (report.Type) {
			case ReportType.RPT_NEW_CLIENT:
				return BuildNewClientReport(report, (DateTime)rptDef.DateStart);

			case ReportType.RPT_PLANNED_PAYTMENT:
				return BuildPlainedPaymentReport(report, (DateTime)rptDef.DateStart);

			case ReportType.RPT_IN_WIZARD:
				return BuildInWizardReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

			case ReportType.RPT_EARNED_INTEREST:
				return BuildEarnedInterestReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

			case ReportType.RPT_LOANS_GIVEN:
				return BuildLoansIssuedReport(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

			default:
				string sReportTitle = report.GetTitle((DateTime)rptDef.DateStart, " ", report.IsDaily ? (DateTime?)null : (DateTime)rptDef.DateEnd);

				return new Div()
					.Append(new H1().Append(new Text(sReportTitle)))
					.Append(TableReport(rptDef, oColumnTypes: oColumnTypes));
			} // switch
		} // GetReportData

		internal Workbook GetWorkBook(System.Web.UI.WebControls.ListItem selectedReport, ReportQuery rptDef, bool isDaily) {
			InitAspose();

			Report report = GetReport(selectedReport.Text);

			if (report == null) {
				var errBook = new Workbook();
				errBook.Worksheets.Clear();
				var se = errBook.Worksheets.Add("Error !!!");
				se.Cells.Merge(1, 1, 1, 6);
				se.Cells[1, 1].PutValue("Error: Type reports for this customer cannot be obtained !!!");
				return errBook;
			} // if

			rptDef.Report = report;
			rptDef.StoredProcedure = report.StoredProcedure;

			report.IsDaily = isDaily;

			switch (report.Type) {
			case ReportType.RPT_NEW_CLIENT:
				return BuildNewClientXls(report, (DateTime)rptDef.DateStart);

			case ReportType.RPT_PLANNED_PAYTMENT:
				return BuildPlainedPaymentXls(report, (DateTime)rptDef.DateStart);

			case ReportType.RPT_IN_WIZARD:
				return BuildInWizardXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

			case ReportType.RPT_EARNED_INTEREST:
				return BuildEarnedInterestXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

			case ReportType.RPT_LOANS_GIVEN:
				return BuildLoansIssuedXls(report, (DateTime)rptDef.DateStart, (DateTime)rptDef.DateEnd);

			default:
				var xlsTitle = report.GetTitle((DateTime)rptDef.DateStart, " ", report.IsDaily ? (DateTime?)null : (DateTime)rptDef.DateEnd);
				return XlsReport(rptDef, xlsTitle);
			} // switch
		} // GetWorkBook

		public Report GetReport(string title) {
			return reportList.Values.FirstOrDefault(report => report.Title.Equals(title));
		} // GetReport

		private SortedDictionary<string, Report> reportList;
	} // class WebReportHandler
} // EzReportsWeb

