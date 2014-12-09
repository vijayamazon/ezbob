namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {

		public ATag BuildStrategyRunningTimeReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			StrategyRunningTime.StrategyRunningTime rpt = new StrategyRunningTime.StrategyRunningTime(DB, this);
			KeyValuePair<ReportQuery, DataTable> oData = rpt.Run(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildStrategyRunningTimeReport

		public ExcelPackage BuildStrategyRunningTimeXls(Report report, DateTime today, DateTime tomorrow) {
			StrategyRunningTime.StrategyRunningTime rpt = new StrategyRunningTime.StrategyRunningTime(DB, this);
			KeyValuePair<ReportQuery, DataTable> pair = rpt.Run(report, today, tomorrow);

			return AddSheetToExcel(pair.Value, report.GetTitle(today, oToDate: tomorrow));
		} // BuildStrategyRunningTimeXls

	} // class BaseReportHandler
} // namespace
