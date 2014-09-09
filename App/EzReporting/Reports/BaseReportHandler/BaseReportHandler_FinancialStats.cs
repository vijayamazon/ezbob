namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Html;
	using Html.Attributes;
	using Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {
		#region public

		#region method BuildFinancialStatsReport

		public ATag BuildFinancialStatsReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateFinancialStatsReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildFinancialStatsReport

		#endregion method BuildFinancialStatsReport

		#region method BuildFinancialStatsXls

		public ExcelPackage BuildFinancialStatsXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateFinancialStatsReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildFinancialStatsXls

		#endregion method BuildFinancialStatsXls

		#endregion public

		#region private

		#region method CreateFinancialStatsReport

		private KeyValuePair<ReportQuery, DataTable> CreateFinancialStatsReport(Report report, DateTime today, DateTime tomorrow) {
			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			DataTable oOutput = rpt.Execute(DB);
			
			var ea = new EarnedInterest.EarnedInterest(DB, EarnedInterest.EarnedInterest.WorkingMode.ForPeriod, false, today, tomorrow, this);
			SortedDictionary<int, decimal> earned = ea.Run();

			oOutput.Rows.Add(0, "Earned interest", earned.Sum(pair => pair.Value));

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateFinancialStatsReport

		#endregion method CreateFinancialStatsReport

		#endregion private
	} // class BaseReportHandler
} // namespace Reports
