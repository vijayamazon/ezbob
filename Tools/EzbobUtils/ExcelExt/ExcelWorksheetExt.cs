namespace Ezbob.ExcelExt {
	using System;
	using System.Drawing;
	using System.Globalization;
	using Ezbob.Utils.ParsedValue;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	public static class ExcelWorksheetExt {
		public static int SetCellTitle(this ExcelWorksheet oSheet, int nRow, int nColumn, object oRaw) {
			return SetCellValue(
				oSheet,
				nRow,
				nColumn,
				oRaw,
				bIsBold: true,
				bSetZebra: false,
				oFontColour: Color.White,
				oBgColour: Color.FromArgb(0x67, 0xc1, 0x0b)
			);
		} // SetCellTitle

		public static int SetRowTitles(this ExcelWorksheet oSheet, int nRow, params object[] aryRaw) {
			int nColumn = 1;

			for (int i = 0; i < aryRaw.Length; i++)
				nColumn = oSheet.SetCellTitle(nRow, nColumn, aryRaw[i]);

			return nColumn;
		} // SetRowTitles

		public static int SetCellValue(
			this ExcelWorksheet oSheet,
			int nRow,
			int nColumn,
			object oRaw,
			bool? bIsBold = false,
			bool? bSetZebra = true,
			Color? oFontColour = null,
			Color? oBgColour = null,
			string sNumberFormat = null,
			bool wrapText = false
		) {
			return oSheet.Cells[nRow, nColumn].SetCellValue(
				oRaw,
				bIsBold,
				bSetZebra,
				oFontColour,
				oBgColour,
				sNumberFormat,
				wrapText
			);
		} // SetCellValue

		public static int SetRowValues(this ExcelWorksheet oSheet, int nRow, bool bSetZebra, params object[] aryRaw) {
			int nColumn = 1;

			for (int i = 0; i < aryRaw.Length; i++)
				nColumn = oSheet.SetCellValue(nRow, nColumn, aryRaw[i], bSetZebra: bSetZebra);

			return nColumn;
		} // SetRowValues
	} // class ExcelWorksheetExt
} // namespace
