namespace Reports.Alibaba {
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

		#region method FindOrCreateSheet

		public static ExcelWorksheet FindOrCreateSheet(this ExcelPackage oReport, string sSheetName, bool bAddCustomerIDColumn, params string[] oColumnNames) {
			return oReport.Workbook.Worksheets[sSheetName] ?? oReport.CreateSheet(sSheetName, bAddCustomerIDColumn, oColumnNames);
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
