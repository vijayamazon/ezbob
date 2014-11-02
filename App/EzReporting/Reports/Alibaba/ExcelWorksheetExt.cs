namespace Reports.Alibaba {
	using System;
	using System.Drawing;
	using System.Globalization;
	using Ezbob.Utils.ParsedValue;
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

		#region method SetRowTitles

		public static int SetRowTitles(this ExcelWorksheet oSheet, int nRow, params object[] aryRaw) {
			int nColumn = 1;

			for (int i = 0; i < aryRaw.Length; i++)
				nColumn = oSheet.SetCellTitle(nRow, nColumn, aryRaw[i]);

			return nColumn;
		} // SetRowTitles

		#endregion method SetRowValues

		#region method SetCellValue

		public static int SetCellValue(this ExcelWorksheet oSheet, int nRow, int nColumn, object oRaw, bool bSetZebra = true) {
			ExcelRange oCell = oSheet.Cells[nRow, nColumn];

			if (oRaw == null)
				oCell.Value = null;
			else {
				if (oRaw.GetType() == typeof (ParsedValue))
					oRaw = ((ParsedValue)oRaw).Raw;

				if (oRaw == null)
					oCell.Value = null;
				else if (oRaw is DateTime)
					oCell.Value = ((DateTime)oRaw).ToString("dd/MMMM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
				else
					oCell.Value = oRaw;
			} // if

			if (bSetZebra && (nRow % 2 != 0)) {
				oCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
				oCell.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
			} // if

			nColumn++;

			return nColumn;
		} // SetCellValue

		#endregion method SetCellValue

		#region method SetRowValues

		public static int SetRowValues(this ExcelWorksheet oSheet, int nRow, bool bSetZebra, params object[] aryRaw) {
			int nColumn = 1;

			for (int i = 0; i < aryRaw.Length; i++)
				nColumn = oSheet.SetCellValue(nRow, nColumn, aryRaw[i], bSetZebra);

			return nColumn;
		} // SetRowValues

		#endregion method SetRowValues
	} // class ExcelWorksheetExt
} // namespace
