namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Logger;
	using Html;
	using Html.Attributes;
	using Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {
		#region method BuildStrategyRunningTimeReport

		public ATag BuildStrategyRunningTimeReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			StrategyRunningTime.StrategyRunningTime rpt = new StrategyRunningTime.StrategyRunningTime(DB, this);
			KeyValuePair<ReportQuery, DataTable> oData = rpt.Run(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildStrategyRunningTimeReport

		#endregion method BuildStrategyRunningTimeReport

		#region method BuildStrategyRunningTimeXls

		public ExcelPackage BuildStrategyRunningTimeXls(Report report, DateTime today, DateTime tomorrow) {
			StrategyRunningTime.StrategyRunningTime rpt = new StrategyRunningTime.StrategyRunningTime(DB, this);
			KeyValuePair<ReportQuery, DataTable> pair = rpt.Run(report, today, tomorrow);

			return AddSheetToExcel(pair.Value, report.GetTitle(today, oToDate: tomorrow));
		} // BuildStrategyRunningTimeXls

		#endregion method BuildStrategyRunningTimeXls
	} // class BaseReportHandler
} // namespace
