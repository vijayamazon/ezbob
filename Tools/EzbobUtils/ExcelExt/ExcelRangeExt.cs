namespace Ezbob.ExcelExt {
	using System;
	using System.Drawing;
	using System.Globalization;
	using Ezbob.Utils.ParsedValue;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	public static class ExcelRangeExt {
		public static int SetCellTitle(this ExcelRange range, object oRaw) {
			return SetCellValue(
				range,
				oRaw,
				bIsBold: true,
				bSetZebra: false,
				oFontColour: ExcelWorksheetExt.ColumnTitleFgColour,
				oBgColour: ExcelWorksheetExt.ColumnTitleBgColour
			);
		} // SetCellTitle

		public static int SetCellValue(
			this ExcelRange oCell,
			object oRaw,
			bool? bIsBold = false,
			bool? bSetZebra = true,
			Color? oFontColour = null,
			Color? oBgColour = null,
			string sNumberFormat = null,
			bool wrapText = false
		) {
			int nRow = oCell.Start.Row;

			object cellValue = null;

			if (oRaw == null)
				cellValue = null;
			else {
				if (oRaw.GetType() == typeof (ParsedValue))
					oRaw = ((ParsedValue)oRaw).Raw;

				if (oRaw == null)
					cellValue = null;
				else if (oRaw is DateTime)
					cellValue = ((DateTime)oRaw).ToString("dd/MMMM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
				else
					cellValue = oRaw;
			} // if

			if (wrapText) {
				oCell.Value = null;
				oCell.RichText.Add(cellValue == null ? string.Empty : cellValue.ToString());
				oCell.Style.WrapText = true;
			} else
				oCell.Value = cellValue;

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

			return oCell.Start.Column + 1;
		} // SetCellValue
	} // class ExcelRangeExt
} // namespace
