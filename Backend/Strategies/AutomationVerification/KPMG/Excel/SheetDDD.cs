namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel {
	using System.Collections.Generic;
	using System.Drawing;
	using DbConstants;
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

		public void Generate(int lastDecisionRow, int lastRawAutomationRow) {
			int row = GenerateNewOldCustomersStats(1, lastDecisionRow);
			GenerateRawAutomationStats(row + 1, lastRawAutomationRow);
		} // Generate

		private void GenerateRawAutomationStats(int row, int lastRawAutomationRow) {
			int column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("All cash requests", true, false, oBgColour: Color.Yellow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Live", true, false, oBgColour: Color.Yellow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Count", true, false, oBgColour: Color.Yellow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Total count", true, false, oBgColour: Color.Yellow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Ratio", true, false, oBgColour: Color.Yellow);

			row++;

			var lst = new List<List<CellCfg>>(); 

			int firstRow = row;

			List<CellCfg> thisRow;

			foreach (KeyValuePair<DecisionActions, int> pair in liveValues) {
				column = 0;

				thisRow = new List<CellCfg> {
					new CellCfg(this.sheet, row, ++column, pair.Key.ToString(), null),
					new CellCfg(this.sheet, row, ++column, "=" + pair.Value, TitledValue.Format.Int, true, fgColour: Color.Gray),
					new CellCfg(this.sheet, row, ++column, "=COUNTIF('Raw automation'!$C$2:$C$" + lastRawAutomationRow + ", A" + row + ")", TitledValue.Format.Int, fgColour: Color.DodgerBlue),
					new CellCfg(this.sheet, row, ++column, "=B" + row + "+C" + row, TitledValue.Format.Int, true, fgColour: Color.Red),
					new CellCfg(this.sheet, row, ++column, "=IF(D" + CellCfg.AutoTotalRow + "=0,0,D" + row + "/D" + CellCfg.AutoTotalRow + ")", TitledValue.Format.Percent),
				};

				lst.Add(thisRow);

				row++;
			} // for each

			int totalRow = row;

			column = 0;

			thisRow = new List<CellCfg> {
				new CellCfg(this.sheet, row, ++column, "Total", null, true, Color.Bisque),
				new CellCfg(this.sheet, row, ++column, "=SUM(B" + firstRow + ":B" + (totalRow - 1) + ")", TitledValue.Format.Int, true, Color.Bisque),
				new CellCfg(this.sheet, row, ++column, "=SUM(C" + firstRow + ":C" + (totalRow - 1) + ")", TitledValue.Format.Int, true, Color.Bisque),
				new CellCfg(this.sheet, row, ++column, "=SUM(D" + firstRow + ":D" + (totalRow - 1) + ")", TitledValue.Format.Int, true, Color.Bisque),
				new CellCfg(this.sheet, row, ++column, "=IF(D" + totalRow + "=0,0,D" + row + "/D" + totalRow + ")", TitledValue.Format.Percent, true, Color.Bisque),
			};

			lst.Add(thisRow);

			foreach (List<CellCfg> rowData in lst)
				foreach (CellCfg cell in rowData)
					cell.Fill(0, totalRow, 0, 0);
		} // GenerateRawAutomationStats

		private static readonly Dictionary<DecisionActions, int> liveValues = new Dictionary<DecisionActions, int> {
			{ DecisionActions.ReReject, 783 },
			{ DecisionActions.Reject, 2968 },
			{ DecisionActions.ReApprove, 724 },
			{ DecisionActions.Approve, -711 },
			{ DecisionActions.Waiting, 0 },
		};

		private int GenerateNewOldCustomersStats(int row, int lastRawRow) {
			AStatItem.SetBorders(this.sheet.Cells[row, 1]).SetCellValue(
				"Customers",
				bSetZebra: false,
				bIsBold: true,
				oBgColour: Color.Yellow
			);

			ExcelRange range = AStatItem.SetBorders(this.sheet.Cells[row, 2, row, 5]);
			range.SetCellValue("New");
			range.Merge = true;
			range.Style.Font.Bold = true;
			range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, 6, row, 9]);
			range.SetCellValue("Returning");
			range.Merge = true;
			range.Style.Font.Bold = true;
			range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);

			range = AStatItem.SetBorders(this.sheet.Cells[row, 10, row, 13]);
			range.SetCellValue("Total");
			range.Merge = true;
			range.Style.Font.Bold = true;
			range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);

			row++;

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

			return row;
		} // GenerateNewOldCustomersStats

		private int CreateAutoPatterns(int row, int lastRawRow, List<List<CellCfg>> cells) {
			int col;

			cells.Add(CreateSectionTitle(row, "Auto decisions"));
			row++;

			this.autoFirstRow = row;

			List<CellCfg> thisRow;

			foreach (string decision in this.autoDecisionNames) {
				string count = CountFormulaPattern
					.Replace(SheetName, this.decisionSheetName)
					.Replace(LastRawRow, lastRawRow.ToString())
					.Replace(DecisionColumn, AutoDecisionColumn)
					.Replace(CurrentRow, row.ToString());

				string sum = decision == "Approve"
					? SumFormulaPattern
						.Replace(SheetName, this.decisionSheetName)
						.Replace(LastRawRow, lastRawRow.ToString())
						.Replace(DecisionColumn, AutoDecisionColumn)
						.Replace(CurrentRow, row.ToString())
					: string.Empty;

				string totalSum = decision == "Approve"
					? TotalSumFormulaPattern.Replace(CurrentRow, row.ToString())
					: string.Empty;

				string ratio = RatioFormulaPattern
					.Replace(TotalRow, CellCfg.AutoTotalRow)
					.Replace(CurrentRow, row.ToString());

				col = 0;

				thisRow = new List<CellCfg> {
					new CellCfg(this.sheet, row, ++col, decision, null),

					new CellCfg(this.sheet, row, ++col, count.Replace(IsNew, NewCustomer), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, ++col, ratio.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, ++col, sum.Replace(IsNew, NewCustomer).Replace(AmountColumn, MinOfferAmountColumn), TitledValue.Format.Money),
					new CellCfg(this.sheet, row, ++col, sum.Replace(IsNew, NewCustomer).Replace(AmountColumn, MaxOfferAmountColumn), TitledValue.Format.Money),

					new CellCfg(this.sheet, row, ++col, count.Replace(IsNew, OldCustomer), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, ++col, ratio.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, ++col, sum.Replace(IsNew, OldCustomer).Replace(AmountColumn, MinOfferAmountColumn), TitledValue.Format.Money),
					new CellCfg(this.sheet, row, ++col, sum.Replace(IsNew, OldCustomer).Replace(AmountColumn, MaxOfferAmountColumn), TitledValue.Format.Money),

					new CellCfg(this.sheet, row, ++col, TotalColumnCountFormulaPattern.Replace(CurrentRow, row.ToString()), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, ++col, ratio.Replace(DataColumn, TotalCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, ++col, totalSum.Replace(MinOfferColumn, NewCustomerMinOfferColumn).Replace(MaxOfferColumn, NewCustomerMaxOfferColumn), TitledValue.Format.Money),
					new CellCfg(this.sheet, row, ++col, totalSum.Replace(MinOfferColumn, OldCustomerMinOfferColumn).Replace(MaxOfferColumn, OldCustomerMaxOfferColumn), TitledValue.Format.Money),
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

			col = 0;

			thisRow = new List<CellCfg> {
				new CellCfg(this.sheet, row, ++col, "Total", null, true, Color.AntiqueWhite),

				new CellCfg(this.sheet, row, ++col, totalCount.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, totalRatio.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),

				new CellCfg(this.sheet, row, ++col, totalCount.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, totalRatio.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),

				new CellCfg(this.sheet, row, ++col, TotalColumnCountFormulaPattern.Replace(CurrentRow, row.ToString()), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, totalRatio.Replace(DataColumn, TotalCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),
			};

			cells.Add(thisRow);

			return row + 1;
		} // CreateAutoPatterns

		private int CreateManualPatterns(int row, int lastRawRow, List<List<CellCfg>> cells) {
			int col;

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

				col = 0;

				thisRow = new List<CellCfg> {
					new CellCfg(this.sheet, row, ++col, decision, null),

					new CellCfg(this.sheet, row, ++col, count.Replace(IsNew, NewCustomer), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, ++col, ratio.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, ++col, null, null),
					new CellCfg(this.sheet, row, ++col, null, null),

					new CellCfg(this.sheet, row, ++col, count.Replace(IsNew, OldCustomer), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, ++col, ratio.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, ++col, null, null),
					new CellCfg(this.sheet, row, ++col, null, null),

					new CellCfg(this.sheet, row, ++col, TotalColumnCountFormulaPattern.Replace(CurrentRow, row.ToString()), TitledValue.Format.Int),
					new CellCfg(this.sheet, row, ++col, ratio.Replace(DataColumn, TotalCustomerColumn), TitledValue.Format.Percent),
					new CellCfg(this.sheet, row, ++col, null, null),
					new CellCfg(this.sheet, row, ++col, null, null),
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

			col = 0;

			thisRow = new List<CellCfg> {
				new CellCfg(this.sheet, row, ++col, "Total", null, true, Color.AntiqueWhite),

				new CellCfg(this.sheet, row, ++col, totalCount.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, totalRatio.Replace(DataColumn, NewCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),

				new CellCfg(this.sheet, row, ++col, totalCount.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, totalRatio.Replace(DataColumn, OldCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),

				new CellCfg(this.sheet, row, ++col, TotalColumnCountFormulaPattern.Replace(CurrentRow, row.ToString()), TitledValue.Format.Int, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, totalRatio.Replace(DataColumn, TotalCustomerColumn), TitledValue.Format.Percent, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),
				new CellCfg(this.sheet, row, ++col, null, null, true, Color.AntiqueWhite),
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
				thisRow.Add(new CellCfg(this.sheet, row, column + i * 4,     "Count",     null, true, Color.Bisque));
				thisRow.Add(new CellCfg(this.sheet, row, column + i * 4 + 1, "Ratio",     null, true, Color.Bisque));
				thisRow.Add(new CellCfg(this.sheet, row, column + i * 4 + 2, "Min offer", null, true, Color.Bisque));
				thisRow.Add(new CellCfg(this.sheet, row, column + i * 4 + 3, "Max offer", null, true, Color.Bisque));
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
		private const string AmountColumn = "__AMOUNT_COLUMN__";
		private const string MinOfferColumn = "__MIN_OFFER_COLUMN__";
		private const string MaxOfferColumn = "__MAX_OFFER_COLUMN__";

		private const string CountFormulaPattern = "=COUNTIFS(" +
			SheetName + "!$CJ$2:$CJ$" + LastRawRow + "," +
			IsNew + "," +
			SheetName + "!$" + DecisionColumn + "$2:$" + DecisionColumn + "$" + LastRawRow +
			",$A$" + CurrentRow +
		")";

		private const string SumFormulaPattern = "=SUMIFS(" +
			SheetName + "!$" + AmountColumn + "$2:$" + AmountColumn + "$" + LastRawRow + "," +
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
		private const string NewCustomerMinOfferColumn = "D";
		private const string NewCustomerMaxOfferColumn = "E";
		private const string OldCustomerColumn = "F";
		private const string OldCustomerMinOfferColumn = "H";
		private const string OldCustomerMaxOfferColumn = "I";
		private const string TotalCustomerColumn = "J";

		private const string AutoDecisionColumn = "Q";
		private const string MinOfferAmountColumn = "BE";
		private const string MaxOfferAmountColumn = "BP";

		private const string NewCustomer = "TRUE";
		private const string OldCustomer = "FALSE";

		private const string TotalColumnCountFormulaPattern =
			"=" + NewCustomerColumn + CurrentRow + "+" + OldCustomerColumn + CurrentRow;

		private const string TotalSumFormulaPattern = "=" + MinOfferColumn + CurrentRow + "+" + MaxOfferColumn + CurrentRow;

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
				Color? bgColour = null,
				Color? fgColour = null
			) {
				this.sheet = sheet;
				this.row = row;
				this.column = column;
				this.formula = formula;
				this.format = format;
				this.isBold = isBold;
				this.bgColour = bgColour;
				this.fgColour = fgColour;
			} // constructor

			public void Fill(
				int autoFirstRow,
				int autoTotalRow,
				int manualFirstRow,
				int manualTotalRow
			) {
				string finalFormula = string.IsNullOrWhiteSpace(this.formula) ? string.Empty : this.formula
					.Replace(AutoFirstRow, autoFirstRow.ToString())
					.Replace(AutoLastRow, (autoTotalRow - 1).ToString())
					.Replace(AutoTotalRow, autoTotalRow.ToString())
					.Replace(ManualFirstRow, manualFirstRow.ToString())
					.Replace(ManualLastRow, (manualTotalRow - 1).ToString())
					.Replace(ManualTotalRow, manualTotalRow.ToString());

				var range = AStatItem.SetBorders(this.sheet.Cells[this.row, this.column]);

				if ((this.column - 2) % 4 == 0)
					range.Style.Border.Left.Style = ExcelBorderStyle.Thick;

				range.SetCellValue(null, this.isBold, false, this.fgColour, this.bgColour, this.format);

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
			private readonly Color? fgColour;
		} // class CellCfg
	} // class SheetDDD
} // namespace
