using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Ezbob.Logger;
using Html;
using Reports;
using Ezbob.Database;
using Aspose.Cells;
using Aspose.Words;

namespace EzReportsWeb {
	public class WebReportHandler : BaseReportHandler {
		public WebReportHandler(AConnection oDB, ASafeLog log = null) : base(oDB, log) {}

		public List<Report> GetReportsList(string userName) {
			DataTable dt = DB.ExecuteReader("RptGetUserReports", new QueryParameter("@UserName", userName));
			//DataTable dt = DbConnection.ExecuteSpReader("RptScheduler_GetReportList"); // to enable all reports - for testing
			reportList = new List<Report>();

			foreach (DataRow row in dt.Rows)
				AddReportToList(reportList, row, DefaultToEMail);

			return reportList;
		} // GetReportsList

		internal ATag GetReportData(System.Web.UI.WebControls.ListItem selectedReport, string fromDate, string toDate, bool isDaily) {
			Report report = GetReport(selectedReport.Text);

			if (report == null)
				return new Span().Append(new Text("Error Occured, try later"));

			report.IsDaily = isDaily;

			switch (report.Type) {
			case ReportType.RPT_NEW_CLIENT:
				return BuildNewClientReport(report, fromDate, toDate);

			case ReportType.RPT_PLANNED_PAYTMENT:
				return BuildPlainedPaymentReport(report, fromDate, toDate);

			case ReportType.RPT_DAILY_STATS:
				return BuildDailyStatsReportBody(report, fromDate, toDate);

			case ReportType.RPT_IN_WIZARD:
				return BuildInWizardReport(report, fromDate, toDate);

			default:
				return BuildReport(report, fromDate, toDate, "");
			} // switch
		} // GetReportData

        internal Workbook GetWorkBook(System.Web.UI.WebControls.ListItem selectedReport, string fromDate, string toDate, bool isDaily)
        {
            InitAspose();            

            Report report = GetReport(selectedReport.Text);
            if (report == null)
            {
                var errBook = new Workbook();
                errBook.Worksheets.Clear();
                var se = errBook.Worksheets.Add("Error !!!");
                se.Cells.Merge(1, 1, 1, 6);
                se.Cells[1, 1].PutValue("Error: Type reports for this customer cannot be obtained !!!");
                return errBook;
            }

            report.IsDaily = isDaily;

            switch (report.Type)
            {
                case ReportType.RPT_NEW_CLIENT:
                            return BuildNewClientXls(report, fromDate, toDate);
                case ReportType.RPT_PLANNED_PAYTMENT:
                            return BuildPlainedPaymentXls(report, fromDate, toDate);
                case ReportType.RPT_DAILY_STATS:
                            return BuildDailyStatsXls(report, fromDate, toDate);
                case ReportType.RPT_IN_WIZARD:
                            return BuildInWizardXls(report, fromDate, toDate);
                    default:
                    return BuildXls(report, fromDate, toDate, "");
            } // switch
        } // GetWorkBook

		private Report GetReport(string title) {
			foreach (Report report in reportList)
				if (report.Title.Equals(title))
					return report;

			return null;
		} // GetReport

		private ATag BuildReport(Report report, string fromDate, string toDate, string period) {
			var oRpt = new Div();

			var h1 = new H1();

			var oRptTitle = new Text(period + " " + report.Title + " " + fromDate);

			oRpt.Append(h1.Append(oRptTitle));

			if (!report.IsDaily)
				oRptTitle.Append(" - " + toDate);

			oRpt.Append(TableReport(report.StoredProcedure, fromDate, toDate, report.Columns));

			return oRpt;
		} // BuildReport

        private Workbook BuildXls(Report report, string fromDate, string toDate, string period)
        {
            var xlsTitle =  period + " " + report.Title + " " + fromDate;
            if (!report.IsDaily) xlsTitle += " - " + toDate;
            return XlsReport(report.StoredProcedure, fromDate, toDate, xlsTitle);
        } // BuildXls

		private List<Report> reportList;
	} // class WebReportHandler
} // EzReportsWeb

