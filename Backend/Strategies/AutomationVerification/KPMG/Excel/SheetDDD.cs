namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel {
	using System.Collections.Generic;
	using System.Drawing;
	using Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	internal class SheetDDD {
		public SheetDDD(
			ExcelPackage workbook,
			string decisionsSheetName,
			SortedSet<string> autoDecisionNames,
 			SortedSet<string> manualDecisionNames 
		) {
			this.sheet = workbook.CreateSheet("Due diligence data", false);
			this.decisionSheetName = decisionsSheetName;
			this.autoDecisionNames = autoDecisionNames;
			this.manualDecisionNames = manualDecisionNames;
		} // constructor

		public void Generate(int lastRawRow) {
			AStatItem.SetBorders(this.sheet.Cells[1, 1]).SetCellValue(
				"Customers",
				bSetZebra: false,
				bIsBold: true,
				oBgColour: Color.Yellow
			);

			ExcelRange range = AStatItem.SetBorders(this.sheet.Cells[1, 2, 1, 3]);
			range.SetCellValue("New");
			range.Merge = true;
			range.Style.Font.Bold = true;
			range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);

			range = AStatItem.SetBorders(this.sheet.Cells[1, 4, 1, 5]);
			range.SetCellValue("Returning");
			range.Merge = true;
			range.Style.Font.Bold = true;
			range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);

			range = AStatItem.SetBorders(this.sheet.Cells[1, 6, 1, 7]);
			range.SetCellValue("Total");
			range.Merge = true;
			range.Style.Font.Bold = true;
			range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);

			int row = 2;
			var cells = new List<List<CellCfg>>();

			row = CreateAutoPatterns(row, lastRawRow, cells);
			row = CreateManualPatterns(row, lastRawRow, cells);

			foreach (List<CellCfg> rowData in cells) {
				foreach (CellCfg cell in rowData) {
					cell.Fill(
						this.autoFirstRow,
						this.autoTotalRow,
						this.manualFirstRow,
						this.manualTotalRow
					);
				} // for each cell
			} // for each row
		} // Generate

		private int CreateAutoPatterns(int row, int lastRawRow, List<List<CellCfg>> cells) {
			cells.Add(CreateSectionTitle(row, "Auto decisions"));
			row++;

			this.autoFirstRow = row;

			List<CellCfg> thisRow;

			foreach (string decision in this.autoDecisionNames) {
				string count = CountFormulaPattern
					.Replace(SheetName, this.decisionSheetName)
					.Replace(LastRawRow, lastRawRow.ToString())
					.Replace(DecisionColumn, "Q")
					.Replace(CurrentRow, row.ToString());

				string ratio = RatioFormulaPattern
					.Replace(TotalRow, CellCfg.AutoTotalRow)
					.Replace(CurrentRow, row.ToString());

				thisRow = new List<CellCfg> {
					new CellCfg(this.sheet, row, 1, decision, null),
					new CellCfg(this.sheet, row, 2, count.Replace(IsNew, "TRUE"), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, 3, ratio.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, 4, count.Replace(IsNew, "FALSE"), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, 5, ratio.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, 6, TotalColumnCountFormulaPattern.Replace(CurrentRow, row.ToString()), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, 7, ratio.Replace(DataColumn, TotalCustomerColumn), TitledValue.Format.Percent),
				};

				cells.Add(thisRow);

				row++;
			} // for each auto decision

			this.autoTotalRow = row;

			string totalCount = TotalRowCountFormulaPattern
				.Replace(FirstRow, CellCfg.AutoFirstRow)
				.Replace(LastRow, CellCfg.AutoLastRow);

			string totalRatio = RatioFormulaPattern
				.Replace(TotalRow, CellCfg.AutoTotalRow)
				.Replace(CurrentRow, row.ToString());

			thisRow = new List<CellCfg> {
				new CellCfg(this.sheet, row, 1, "Total", null, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 2, totalCount.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 3, totalRatio.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 4, totalCount.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 5, totalRatio.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 6, TotalColumnCountFormulaPattern.Replace(CurrentRow, row.ToString()), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 7, totalRatio.Replace(DataColumn, TotalCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
			};

			cells.Add(thisRow);

			return row + 1;
		} // CreateAutoPatterns

		private int CreateManualPatterns(int row, int lastRawRow, List<List<CellCfg>> cells) {
			cells.Add(CreateSectionTitle(row, "Manual decisions"));
			row++;

			this.manualFirstRow = row;

			List<CellCfg> thisRow;

			foreach (string decision in this.manualDecisionNames) {
				string count = CountFormulaPattern
					.Replace(SheetName, this.decisionSheetName)
					.Replace(LastRawRow, lastRawRow.ToString())
					.Replace(DecisionColumn, "J")
					.Replace(CurrentRow, row.ToString());

				string ratio = RatioFormulaPattern
					.Replace(TotalRow, CellCfg.ManualTotalRow)
					.Replace(CurrentRow, row.ToString());

				thisRow = new List<CellCfg> {
					new CellCfg(this.sheet, row, 1, decision, null),
					new CellCfg(this.sheet, row, 2, count.Replace(IsNew, "TRUE"), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, 3, ratio.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, 4, count.Replace(IsNew, "FALSE"), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, 5, ratio.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, 6, TotalColumnCountFormulaPattern.Replace(CurrentRow, row.ToString()), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, 7, ratio.Replace(DataColumn, TotalCustomerColumn), TitledValue.Format.Percent),
				};

				cells.Add(thisRow);

				row++;
			} // for each manual decision

			this.manualTotalRow = row;

			string totalCount = TotalRowCountFormulaPattern
				.Replace(FirstRow, CellCfg.ManualFirstRow)
				.Replace(LastRow, CellCfg.ManualLastRow);

			string totalRatio = RatioFormulaPattern
				.Replace(TotalRow, CellCfg.ManualTotalRow)
				.Replace(CurrentRow, row.ToString());

			thisRow = new List<CellCfg> {
				new CellCfg(this.sheet, row, 1, "Total", null, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 2, totalCount.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 3, totalRatio.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 4, totalCount.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 5, totalRatio.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 6, TotalColumnCountFormulaPattern.Replace(CurrentRow, row.ToString()), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, 7, totalRatio.Replace(DataColumn, TotalCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
			};

			cells.Add(thisRow);

			return row + 1;
		} // CreateManualPatterns

		private List<CellCfg> CreateSectionTitle(int row, string title) {
			var thisRow = new List<CellCfg> {
				new CellCfg(this.sheet, row, 1, title, null, true, Color.Bisque),
			};

			const int column = 2;

			for (int i = 0; i < 3; i++) {
				thisRow.Add(new CellCfg(this.sheet, row, column + i * 2,     "Count", null, true, Color.Bisque));
				thisRow.Add(new CellCfg(this.sheet, row, column + i * 2 + 1, "Ratio", null, true, Color.Bisque));
			} // for

			return thisRow;
		} // CreateSectionTitle

		private readonly ExcelWorksheet sheet;
		private readonly string decisionSheetName;
		private readonly SortedSet<string> autoDecisionNames;
		private readonly SortedSet<string> manualDecisionNames;

		private const string LastRawRow = "__LAST_RAW_ROW__";
		private const string CurrentRow = "__CURRENT_ROW__";
		private const string DecisionColumn = "__DECISION_COLUMN__";
		private const string SheetName = "__SHEET_NAME__";
		private const string IsNew = "__IS_NEW__";

		private const string CountFormulaPattern = "=COUNTIFS(" +
			SheetName + "!$CJ$2:$CJ$" + LastRawRow + "," +
			IsNew + "," +
			SheetName + "!$" + DecisionColumn + "$2:$" + DecisionColumn + "$" + LastRawRow +
			",$A$" + CurrentRow +
		")";

		private const string TotalRow = "__TOTAL__";
		private const string FirstRow = "__FIRST__";
		private const string LastRow = "__LAST__";
		private const string DataColumn = "__DATA_COLUMN__";

		private const string TotalRowCountFormulaPattern =
			"=SUM(" + DataColumn + FirstRow + ":" + DataColumn + LastRow + ")";

		private const string NewCustomerColumn = "B";
		private const string OldCustomerColumn = "D";
		private const string TotalCustomerColumn = "F";

		private const string TotalColumnCountFormulaPattern =
			"=" + NewCustomerColumn + CurrentRow + "+" + OldCustomerColumn + CurrentRow;

		private const string RatioFormulaPattern = "=IF(" +
			"$" + DataColumn + "$" + TotalRow + "=0,0," +
			DataColumn + CurrentRow + "/$" + DataColumn + "$" + TotalRow +
		")";

		private int autoTotalRow;
		private int autoFirstRow;
		private int manualTotalRow;
		private int manualFirstRow;

		private class CellCfg {
			public const string AutoTotalRow = "__AUTO_TOTAL__";
			public const string AutoFirstRow = "__AUTO_FIRST__";
			public const string AutoLastRow = "__AUTO_LAST__";

			public const string ManualTotalRow = "__MANUAL_TOTAL__";
			public const string ManualFirstRow = "__MANUAL_FIRST__";
			public const string ManualLastRow = "__MANUAL_LAST__";

			public CellCfg(
				ExcelWorksheet sheet,
				int row,
				int column,
				string formula,
				string format,
				bool isBold = false,
				Color? bgColour = null
			) {
				this.sheet = sheet;
				this.row = row;
				this.column = column;
				this.formula = formula;
				this.format = format;
				this.isBold = isBold;
				this.bgColour = bgColour;
			} // constructor

			public void Fill(
				int autoFirstRow,
				int autoTotalRow,
				int manualFirstRow,
				int manualTotalRow
			) {
				string finalFormula = this.formula
					.Replace(AutoFirstRow, autoFirstRow.ToString())
					.Replace(AutoLastRow, (autoTotalRow - 1).ToString())
					.Replace(AutoTotalRow, autoTotalRow.ToString())
					.Replace(ManualFirstRow, manualFirstRow.ToString())
					.Replace(ManualLastRow, (manualTotalRow - 1).ToString())
					.Replace(ManualTotalRow, manualTotalRow.ToString());

				var range = AStatItem.SetBorders(this.sheet.Cells[this.row, this.column]);

				if (this.column % 2 == 0)
					range.Style.Border.Left.Style = ExcelBorderStyle.Thick;

				range.SetCellValue(null, this.isBold, false, oBgColour: this.bgColour, sNumberFormat: this.format);

				if (finalFormula.StartsWith("="))
					range.Formula = finalFormula;
				else
					range.Value = finalFormula;
			} // Fill

			private readonly int row;
			private readonly int column;
			private readonly ExcelWorksheet sheet;
			private readonly string formula;
			private readonly string format;
			private readonly bool isBold;
			private readonly Color? bgColour;
		} // class CellCfg
	} // class SheetDDD
} // namespace
