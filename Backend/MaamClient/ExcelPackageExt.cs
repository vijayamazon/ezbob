namespace MaamClient {
	using System;
	using System.Collections.Generic;
	using OfficeOpenXml;

	internal static class ExcelPackageExt {
		public static ExcelWorksheet CreateSheet(this ExcelPackage oReport, string sSheetName, params string[] oColumnNames) {
			var oSheet = oReport.Workbook.Worksheets.Add(sSheetName);

			var lst = new List<object>();

			lst.AddRange(oColumnNames);

			oSheet.SetRowTitles(1, lst.ToArray());

			oSheet.View.ShowGridLines = false;

			return oSheet;
		} // CreateSheet

		public static ExcelWorksheet FindSheet(this ExcelPackage oReport, string sSheetName) {
			return oReport.Workbook.Worksheets[sSheetName];
		} // FindSheet

		public static ExcelWorksheet FindOrCreateSheet(this ExcelPackage oReport, string sSheetName, params string[] oColumnNames) {
			return FindOrCreateSheet(oReport, sSheetName, null, oColumnNames);
		} // FindOrCreateSheet

		public static ExcelWorksheet FindOrCreateSheet(this ExcelPackage oReport, string sSheetName, Action<ExcelWorksheet> OnCreate, params string[] oColumnNames) {
			ExcelWorksheet ws = oReport.Workbook.Worksheets[sSheetName];

			if (ws != null)
				return ws;

			ws = oReport.CreateSheet(sSheetName, oColumnNames);

			if (OnCreate != null)
				OnCreate(ws);

			return ws;
		} // FindOrCreateSheet

		public static void AutoFitColumns(this ExcelPackage oReport) {
			foreach (ExcelWorksheet oSheet in oReport.Workbook.Worksheets) {
				int nColumn = 1;

				while (oSheet.Cells[1, nColumn].Value != null) {
					oSheet.Column(nColumn)
						.AutoFit();
					nColumn++;
				} // while
			} // for each sheet
		} // AutoFitColumns
	} // class ExcelPackageExt
} // namespace
