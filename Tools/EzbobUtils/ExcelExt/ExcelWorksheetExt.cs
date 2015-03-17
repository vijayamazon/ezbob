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
			string sNumberFormat = null
		) {
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

			if (oFontColour.HasValue)
				oCell.Style.Font.Color.SetColor(oFontColour.Value);

			bool bDoZebra = bSetZebra.HasValue && bSetZebra.Value;

			if (bDoZebra) {
				if (nRow % 2 != 0) {
					oCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
					oCell.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
				} // if
			}
			else {
				if (oBgColour.HasValue) {
					oCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
					oCell.Style.Fill.BackgroundColor.SetColor(oBgColour.Value);
				} // if
			} // if

			oCell.Style.Font.Bold = bIsBold.HasValue && bIsBold.Value;

			if (!string.IsNullOrWhiteSpace(sNumberFormat))
				oCell.Style.Numberformat.Format = sNumberFormat;

			nColumn++;

			return nColumn;
		} // SetCellValue

		public static int SetRowValues(this ExcelWorksheet oSheet, int nRow, bool bSetZebra, params object[] aryRaw) {
			int nColumn = 1;

			for (int i = 0; i < aryRaw.Length; i++)
				nColumn = oSheet.SetCellValue(nRow, nColumn, aryRaw[i], bSetZebra: bSetZebra);

			return nColumn;
		} // SetRowValues
	} // class ExcelWorksheetExt
} // namespace
