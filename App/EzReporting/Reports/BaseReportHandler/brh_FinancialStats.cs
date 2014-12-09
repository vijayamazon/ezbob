namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {

		public ATag BuildFinancialStatsReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateFinancialStatsReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildFinancialStatsReport

		public ExcelPackage BuildFinancialStatsXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateFinancialStatsReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildFinancialStatsXls

		private KeyValuePair<ReportQuery, DataTable> CreateFinancialStatsReport(Report report, DateTime today, DateTime tomorrow) {
			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			var ea = new EarnedInterest.EarnedInterest(DB, EarnedInterest.EarnedInterest.WorkingMode.ForPeriod, false, today, tomorrow, this);
			SortedDictionary<int, decimal> earned = ea.Run();

			DataTable oOutput = new DataTable();

			oOutput.Columns.Add("SortOrder", typeof (int));
			oOutput.Columns.Add("Caption", typeof (string));
			oOutput.Columns.Add("Value", typeof (decimal));

			rpt.Execute(DB, (sr, bRowsetStart) => {
				oOutput.Rows.Add((int)sr[0], (string)sr[1], (decimal)sr[2]);
				return ActionResult.Continue;
			});

			oOutput.Rows.Add(0, "Earned interest", earned.Sum(pair => pair.Value));

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateFinancialStatsReport

	} // class BaseReportHandler
} // namespace Reports
