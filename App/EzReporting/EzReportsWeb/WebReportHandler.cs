using System.Data;
using System.Collections.Generic;
using System.Text;
using Ezbob.Logger;
using Reports;
using Ezbob.Database;

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

		internal string GetReportData(System.Web.UI.WebControls.ListItem selectedReport, string fromDate, string toDate, bool isDaily) {
			Report report = GetReport(selectedReport.Text);
			if (report == null)
				return "<span>Error Occured, try later</span>";

			report.IsDaily = isDaily;
			string reportData;

			switch (report.Type) {
			case ReportType.RPT_NEW_CLIENT:
				reportData = BuildNewClientReport(report, fromDate, toDate);
				break;
			case ReportType.RPT_PLANNED_PAYTMENT:
				reportData = BuildPlainedPaymentReport(report, fromDate, toDate);
				break;
			case ReportType.RPT_DAILY_STATS:
				reportData = BuildDailyStatsReportBody(report, fromDate, toDate);
				break;
			case ReportType.RPT_IN_WIZARD:
				reportData = BuildInWizardReport(report, fromDate, toDate);
				break;
			default:
				reportData = HandleGenericReport(report, fromDate, toDate);
				break;
			} // switch

			return reportData;
		} // GetReportData

		private Report GetReport(string title) {
			foreach (Report report in reportList)
				if (report.Title.Equals(title))
					return report;

			return null;
		} // GetReport

		private string HandleGenericReport(Report report, string fromDate, string toDate) {
			return BuildReport(report, fromDate, toDate, "");
		} // HandleGenericReport

		private string BuildReport(Report report, string fromDate, string toDate, string period) {
			var bodyText = new StringBuilder();
			if (report.IsDaily)
				bodyText.Append("<body><h1> " + period + " " + report.Title + " " + fromDate + "</h1>");
			else
				bodyText.Append("<body><h1> " + period + " " + report.Title + " " + fromDate + " - " + toDate + "</h1>");

			TableReport(bodyText, report.StoredProcedure, fromDate, toDate, report.Headers, report.Fields);
			bodyText.Append("</body>");

			return bodyText.ToString();
		} // BuildReport

		private List<Report> reportList;
	} // class WebReportHandler
} // EzReportsWeb

