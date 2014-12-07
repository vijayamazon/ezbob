namespace EzReportsWeb {
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Tags;
	using OfficeOpenXml;
	using Reports;
	using Ezbob.Database;

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

			return GetHtml(rptDef, oColumnTypes, report);
		} // GetReportData

		internal ExcelPackage GetWorkBook(System.Web.UI.WebControls.ListItem selectedReport, ReportQuery rptDef, bool isDaily)
		{
			Report report = GetReport(selectedReport.Text);

			if (report == null)
				return ErrorXlsReport("Type reports for this customer cannot be obtained !!!");

			rptDef.Report = report;
			rptDef.StoredProcedure = report.StoredProcedure;

			report.IsDaily = isDaily;

			return GetXls(rptDef, report);
		} // GetWorkBook

		public Report GetReport(string title)
		{
			return ReportList.Values.FirstOrDefault(report => report.Title.Equals(title));
		} // GetReport

		public SortedDictionary<string, Report> ReportList { get; private set; }
	} // class WebReportHandler
} // EzReportsWeb

