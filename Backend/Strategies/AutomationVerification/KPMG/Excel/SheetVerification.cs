namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel {
	using System.Drawing;
	using Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	internal class SheetVerification {
		public SheetVerification(ExcelPackage workbook) {
			this.sheet = workbook.CreateSheet("Verification", false);
		} // constructor

		public void Generate(int lastDecisionRow) {
			int curRow = DrawVerificationData(1, lastDecisionRow);
			curRow = DrawConfiguration(curRow + 1);
			curRow = DrawTotalAndReject(curRow + 1, lastDecisionRow);

			DrawDefaultsData(curRow + 1, lastDecisionRow);
		} // Generate

		private int DrawVerificationData(int row, int lastRawRow) {
			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, 3]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Verification data", bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			this.sheet.SetCellValue(row, 2, "Reference", true);
			this.sheet.SetCellValue(row, 3, "Actual", true);
			row++;

			row = DrawVerificationRow(
				row,
				"Approve count",
				Reference.Approve.Count,
				string.Format("=COUNTIF(Decisions!$J$2:$J${0},\"Approved\") ", lastRawRow),
				TitledValue.Format.Int
			);

			row = DrawVerificationRow(
				row,
				"Approve amount",
				Reference.Approve.Amount,
				string.Format("=SUMIF(Decisions!$J$2:$J${0},\"Approved\",Decisions!$K$2:$K${0})", lastRawRow),
				TitledValue.Format.Money
			);

			row = DrawVerificationRow(
				row,
				"Loan count",
				Reference.Loan.Count,
				string.Format("=SUM(Decisions!$BA$2:$BA${0})", lastRawRow),
				TitledValue.Format.Int
			);

			row = DrawVerificationRow(
				row,
				"Loan amount",
				Reference.Loan.Amount,
				string.Format("=SUM(Decisions!$BB$2:$BB${0})", lastRawRow),
				TitledValue.Format.Money
			);

			row = DrawVerificationRow(
				row,
				"Default count",
				Reference.Default.Count,
				string.Format("=COUNTIF(Decisions!$D$2:$D${0}, \"Default\")", lastRawRow),
				TitledValue.Format.Int
			);

			row = DrawVerificationRow(
				row,
				"Default issued amount",
				Reference.Default.Issued.Amount,
				string.Format("=SUMIF(Decisions!$D$2:$D${0}, \"Default\",Decisions!$BB$2:$BB${0})", lastRawRow),
				TitledValue.Format.Money
			);

			row = DrawVerificationRow(
				row,
				"Default outstanding amount",
				Reference.Default.Outstanding.Amount,
				string.Format("=SUMIF(Decisions!$D$2:$D${0}, \"Default\",Decisions!$BD$2:$BD${0})", lastRawRow),
				TitledValue.Format.Money
			);

			return row;
		} // DrawVerificationData

		private int DrawVerificationRow(
			int row,
			string title,
			decimal reference,
			string actualFormula,
			string format
		) {
			this.sheet.SetCellValue(row, 1, title, true);
			this.sheet.SetCellValue(row, 2, reference, sNumberFormat: format);
			this.sheet.SetCellValue(row, 3, null, sNumberFormat: format);
			this.sheet.Cells[row, 3].Formula = actualFormula;

			ExcelAddress rangeToApply = new ExcelAddress(row, 3, row, 3);

			var areEqual = this.sheet.ConditionalFormatting.AddExpression(rangeToApply);
			areEqual.Style.Font.Color.Color = Color.DarkGreen;
			areEqual.Formula = string.Format("B{0}=C{0}", row);

			var areNotEqual = this.sheet.ConditionalFormatting.AddExpression(rangeToApply);
			areNotEqual.Style.Font.Color.Color = Color.Red;
			areNotEqual.Formula = string.Format("B{0}<>C{0}", row);

			return row + 1;
		} // DrawVerificationRow

		private int DrawConfiguration(int row) {
			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, 2]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Report configuration", bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			row = DrawConfigurationRow(row, "First auto approve top limitation", 15000, TitledValue.Format.Money);
			row = DrawConfigurationRow(row, "Second auto approve top limitation", 20000, TitledValue.Format.Money);
			row = DrawConfigurationRow(row, "Default issued rate (% of loans) - amount", 0.2, TitledValue.Format.Percent);
			row = DrawConfigurationRow(row, "Default issued rate (% of loans) - count", 0.2, TitledValue.Format.Percent);
			row = DrawConfigurationRow(row, "Home owner cap", 120000, TitledValue.Format.Money);
			row = DrawConfigurationRow(row, "Homeless cap", 20000, TitledValue.Format.Money);
			row = DrawConfigurationRow(row, "Round to", 100, TitledValue.Format.Int);

			return row;
		} // DrawConfiguration

		private int DrawConfigurationRow(int row, string title, object cfgVal, string format) {
			this.sheet.SetCellValue(row, 1, title);
			this.sheet.SetCellValue(row, 2, cfgVal, sNumberFormat: format);
			return row + 1;
		} // DrawConfigurationRow

		// ReSharper disable once UnusedMethodReturnValue.Local
		private int DrawTotalAndReject(int row, int lastRawRow) {
			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, 2]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Total and Reject data", bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			int totalRow = row;

			row = DrawTotalAndRejectRow(row,
				"Total count", "=COUNT(Decisions!$A$2:$A{0})", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectTitle(row, "Auto processed");

			int autoProcessedRow = row;

			row = DrawTotalAndRejectRow(row,
				"Count", "=COUNTIFS(Decisions!$Q$2:$Q${0},\"<>Waiting\")", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectRow(row,
				"Processed / total %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", totalRow, autoProcessedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectTitle(row, "Auto rejected");

			int autoRejectedRow = row;

			row = DrawTotalAndRejectRow(row,
				"Count", "=COUNTIF(Decisions!$Q$2:$Q${0},\"Reject\")", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectRow(row,
				"Rejected / total %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", totalRow, autoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectRow(row,
				"Rejected / Processed %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", autoProcessedRow, autoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectTitle(row, "Manually rejected");

			int manuallyRejectedRow = row;

			row = DrawTotalAndRejectRow(row,
				"Count", "=COUNTIF(Decisions!$J$2:$J${0},\"Rejected\")", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectRow(row,
				"Rejected / Processed %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", totalRow, manuallyRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectTitle(row, "Manually and auto rejected");

			int manuallyAndAutoRejectedRow = row;

			row = DrawTotalAndRejectRow(row,
				"Count",
				"=COUNTIFS(Decisions!$J$2:$J${0},\"Rejected\", Decisions!$Q$2:$Q${0},\"Reject\")",
				lastRawRow,
				TitledValue.Format.Int
			);

			row = DrawTotalAndRejectRow(row,
				"Rejected / total %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", totalRow, manuallyAndAutoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectRow(row,
				"Rejected / processed %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", autoProcessedRow, manuallyAndAutoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectRow(row,
				"Rejected / manually rejected %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", manuallyRejectedRow, manuallyAndAutoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectRow(row,
				"Rejected / auto rejected %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", autoRejectedRow, manuallyAndAutoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			return row;
		} // DrawVerificationData

		private int DrawTotalAndRejectRow(
			int row,
			string title,
			string formulaPattern,
			int? lastRawRow,
			string valueFormat
		) {
			this.sheet.SetCellValue(row, 1, title, bSetZebra: false);

			var cell = this.sheet.Cells[row, 2];

			cell.SetCellValue(null, bSetZebra: false, sNumberFormat: valueFormat);

			cell.Formula = lastRawRow == null ? formulaPattern : string.Format(formulaPattern, lastRawRow.Value);

			return row + 1;
		} // DrawTotalAndRejectRow

		// ReSharper disable once UnusedMethodReturnValue.Local
		private int DrawDefaultsData(int row, int lastRawRow) {
			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, 5]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Defaults: auto vs manual", bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			AStatItem.SetBorders(this.sheet.Cells[row, 1]);

			var range = AStatItem.SetBorders(this.sheet.Cells[row, 2, row, 3]);
			range.Merge = true;
			range.SetCellValue("Min offer", true, false);

			range = AStatItem.SetBorders(this.sheet.Cells[row, 4, row, 5]);
			range.Merge = true;
			range.SetCellValue("Max offer", true, false);

			row++;

			int column = 2;

			AStatItem.SetBorders(this.sheet.Cells[row, 1]);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Count", true, false, oBgColour: Color.Bisque);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Amount", true, false, oBgColour: Color.Bisque);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Count", true, false, oBgColour: Color.Bisque);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Amount", true, false, oBgColour: Color.Bisque);

			row++;

			column = 2;

			AStatItem.SetBorders(this.sheet.Cells[row, 1]).SetCellValue("Defaults: auto < manual", bSetZebra: false);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"Default\",Decisions!$CL$2:$CL${0},\">0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$CL$2:$CL${0},Decisions!$D$2:$D${0},\"Default\",Decisions!$CL$2:$CL${0},\">0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"Default\",Decisions!$CM$2:$CM${0},\">0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$CM$2:$CM${0},Decisions!$D$2:$D${0},\"Default\",Decisions!$CM$2:$CM${0},\">0\")", lastRawRow);

			row++;

			column = 2;

			AStatItem.SetBorders(this.sheet.Cells[row, 1]).SetCellValue("Defaults: auto = manual", bSetZebra: false);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"Default\",Decisions!$CL$2:$CL${0},\"0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$BE$2:$BE${0},Decisions!$D$2:$D${0},\"Default\",Decisions!$CL$2:$CL${0},\"0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"Default\",Decisions!$CM$2:$CM${0},\"0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$BP$2:$BP${0},Decisions!$D$2:$D${0},\"Default\",Decisions!$CM$2:$CM${0},\"0\")", lastRawRow);

			row++;

			column = 2;

			AStatItem.SetBorders(this.sheet.Cells[row, 1]).SetCellValue("Defaults: auto > manual", bSetZebra: false);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"Default\",Decisions!$CL$2:$CL${0},\"<0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$CL$2:$CL${0},Decisions!$D$2:$D${0},\"Default\",Decisions!$CL$2:$CL${0},\"<0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"Default\",Decisions!$CM$2:$CM${0},\"<0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$CM$2:$CM${0},Decisions!$D$2:$D${0},\"Default\",Decisions!$CM$2:$CM${0},\"<0\")", lastRawRow);

			row++;

			column = 2;

			AStatItem.SetBorders(this.sheet.Cells[row, 1]).SetCellValue("Non-defaults: auto > manual", bSetZebra: false);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"No\",Decisions!$CL$2:$CL${0},\"<0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=-SUMIFS(Decisions!$CL$2:$CL${0},Decisions!$D$2:$D${0},\"No\",Decisions!$CL$2:$CL${0},\"<0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"No\",Decisions!$CM$2:$CM${0},\"<0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=-SUMIFS(Decisions!$CM$2:$CM${0},Decisions!$D$2:$D${0},\"No\",Decisions!$CM$2:$CM${0},\"<0\")", lastRawRow);

			row++;

			column = 2;

			AStatItem.SetBorders(this.sheet.Cells[row, 1]).SetCellValue("Non-defaults: auto = manual", bSetZebra: false);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"No\",Decisions!$CL$2:$CL${0},\"0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$BE$2:$BE${0},Decisions!$D$2:$D${0},\"No\",Decisions!$CL$2:$CL${0},\"0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"No\",Decisions!$CM$2:$CM${0},\"0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$BP$2:$BP${0},Decisions!$D$2:$D${0},\"No\",Decisions!$CM$2:$CM${0},\"0\")", lastRawRow);

			row++;

			column = 2;

			AStatItem.SetBorders(this.sheet.Cells[row, 1]).SetCellValue("Non-defaults: auto < manual", bSetZebra: false);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"No\",Decisions!$CL$2:$CL${0},\">0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$CL$2:$CL${0},Decisions!$D$2:$D${0},\"No\",Decisions!$CL$2:$CL${0},\">0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Int);
			range.Formula = string.Format("=COUNTIFS(Decisions!$D$2:$D${0},\"No\",Decisions!$CM$2:$CM${0},\">0\")", lastRawRow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, column]);
			column = range.SetCellValue(null, bSetZebra: false, sNumberFormat: TitledValue.Format.Money);
			range.Formula = string.Format("=SUMIFS(Decisions!$CM$2:$CM${0},Decisions!$D$2:$D${0},\"No\",Decisions!$CM$2:$CM${0},\">0\")", lastRawRow);

			row++;

			return row;
		} // DrawDefaultsData

		private int DrawTotalAndRejectTitle(int row, string title) {
			ExcelRange range = this.sheet.Cells[row, 1, row, 2];

			range.Merge = true;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Bisque);
			range.Style.Font.Bold = true;
			range.Value = title;

			return row + 1;
		} // DrawTotalAndRejectTitle

		private readonly ExcelWorksheet sheet;
	} // class SheetVerification
} // namespace
