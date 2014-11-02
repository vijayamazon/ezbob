namespace Reports.Alibaba {
	using System;
	using System.Collections.Generic;
	using OfficeOpenXml;

	internal static class ExcelPackageExt {
		#region method CreateSheet

		public static ExcelWorksheet CreateSheet(this ExcelPackage oReport, string sSheetName, bool bAddCustomerIDColumn, params string[] oColumnNames) {
			var oSheet = oReport.Workbook.Worksheets.Add(sSheetName);

			var lst = new List<object>();

			if (bAddCustomerIDColumn) {
				lst.Add("Ezbob Customer ID");
				lst.Add("Alibaba Customer ID");
			} // if

			lst.AddRange(oColumnNames);

			oSheet.SetRowTitles(1, lst.ToArray());

			oSheet.View.ShowGridLines = false;

			return oSheet;
		} // CreateSheet

		#endregion method CreateSheet

		#region method FindSheet

		public static ExcelWorksheet FindSheet(this ExcelPackage oReport, string sSheetName) {
			return oReport.Workbook.Worksheets[sSheetName];
		} // FindSheet

		#endregion method FindSheet

		#region method FindOrCreateSheet

		public static ExcelWorksheet FindOrCreateSheet(this ExcelPackage oReport, string sSheetName, bool bAddCustomerIDColumn, params string[] oColumnNames) {
			return FindOrCreateSheet(oReport, sSheetName, bAddCustomerIDColumn, null, oColumnNames);
		} // FindOrCreateSheet

		public static ExcelWorksheet FindOrCreateSheet(this ExcelPackage oReport, string sSheetName, bool bAddCustomerIDColumn, Action<ExcelWorksheet> OnCreate, params string[] oColumnNames) {
			ExcelWorksheet ws = oReport.Workbook.Worksheets[sSheetName];

			if (ws != null)
				return ws;

			ws = oReport.CreateSheet(sSheetName, bAddCustomerIDColumn, oColumnNames);

			if (OnCreate != null)
				OnCreate(ws);

			return ws;
		} // FindOrCreateSheet

		#endregion method FindOrCreateSheet

		#region method AutoFitColumns

		public static void AutoFitColumns(this ExcelPackage oReport) {
			foreach (ExcelWorksheet oSheet in oReport.Workbook.Worksheets) {
				int nColumn = 1;

				while (oSheet.Cells[1, nColumn].Value != null) {
					oSheet.Column(nColumn).AutoFit();
					nColumn++;
				} // while
			} // for each sheet
		} // AutoFitColumns

		#endregion method AutoFitColumns
	} // class ExcelPackageExt
} // namespace
