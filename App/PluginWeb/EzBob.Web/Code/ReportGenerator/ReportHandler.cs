namespace EzBob.Web.Code.ReportGenerator {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using OfficeOpenXml;
	using Reports;

	public class ReportHandler : BaseReportHandler {
		public ReportHandler(AConnection oDB, ASafeLog log = null) : base(oDB, log) {
		} // constructor

		internal ATag GetReportData(Report report, ReportQuery rptDef, List<string> oColumnTypes, out bool isError) {
			isError = false;
			return GetHtml(rptDef, oColumnTypes, report);
		} // GetReportData

		internal ExcelPackage GetWorkBook(Report report, ReportQuery rptDef) {
			if (report == null)
				return ErrorXlsReport("Type reports for this customer cannot be obtained !!!");

			rptDef.Report = report;
			rptDef.StoredProcedure = report.StoredProcedure;

			return GetXls(rptDef, report);
		} // GetWorkBook
	}
}