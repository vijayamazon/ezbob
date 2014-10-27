namespace Reports.Alibaba {
	using System;
	using System.Drawing;
	using System.Globalization;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	internal static class ExcelWorksheetExt {
		#region method SetCellTitle

		public static int SetCellTitle(this ExcelWorksheet oSheet, int nRow, int nColumn, object oRaw) {
			oSheet.Cells[nRow, nColumn].Style.Font.Bold = true;

			oSheet.Cells[nRow, nColumn].Style.Font.Color.SetColor(Color.White);

			oSheet.Cells[nRow, nColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
			oSheet.Cells[nRow, nColumn].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0x67, 0xc1, 0x0b));

			return SetCellValue(oSheet, nRow, nColumn, oRaw, false);
		} // SetCellTitle

		#endregion method SetCellTitle

		#region method SetCellValue

		public static int SetCellValue(this ExcelWorksheet oSheet, int nRow, int nColumn, object oRaw, bool bSetZebra = true) {
			ExcelRange oCell = oSheet.Cells[nRow, nColumn]; 

			if (oRaw == null)
				oCell.Value = null;
			else if (oRaw is DateTime)
				oCell.Value = ((DateTime)oRaw).ToString("dd/MMMM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
			else
				oCell.Value = oRaw;

			if (bSetZebra && (nRow % 2 != 0)) {
				oCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
				oCell.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
			} // if

			nColumn++;

			return nColumn;
		} // SetCellValue

		#endregion method SetCellValue
	} // class ExcelWorksheetExt
} // namespace
