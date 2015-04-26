namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;
	using System.Drawing;
	using Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	internal class SheetInterestRate {
		public SheetInterestRate(ExcelPackage workbook, SortedSet<string> scenarioNames) {
			this.scenarioNames = scenarioNames;
			this.sheet = workbook.CreateSheet("Interest rate", false);
		} // constructor

		public void Create(int lastRawRow) {
			int row = 1;

			foreach (var cfg in interestRateSheetSections)
				row = CreateInterestRateSheetSection(row, lastRawRow, cfg);
		} // Create

		private static readonly List<InterestRateSheetSectionConfiguration> interestRateSheetSections =
			new List<InterestRateSheetSectionConfiguration> {
				new InterestRateSheetSectionConfiguration("All decisions", "CG", "<>\"\"", "<>\"\"\"\""),
				new InterestRateSheetSectionConfiguration("EU & COSME loans", "CH", "=\"EU\"", "EU"),
				new InterestRateSheetSectionConfiguration("COSME only", "CG", "=\"COSME\"", "COSME"),
			};

		private int CreateInterestRateSheetSection(int row, int lastRawRow, InterestRateSheetSectionConfiguration cfg) {
			ExcelRange range = AStatItem.SetBorders(this.sheet.Cells[row, 1]);
			range.SetCellValue(cfg.Title, bSetZebra: false);
			range.Style.Font.Size = 16;
			range.Style.Font.Bold = true;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);

			int decisionTypeWidth = interestRateSheetColumns.Count;

			int column = 2;

			foreach (string decisionType in interestRateSheetDecisionTypes) {
				range = AStatItem.SetBorders(this.sheet.Cells[row, column, row, column + decisionTypeWidth - 1]);
				range.Merge = true;
				range.SetCellValue(decisionType, bSetZebra: false);
				range.Style.Font.Size = 16;
				range.Style.Font.Bold = true;
				range.Style.Fill.PatternType = ExcelFillStyle.Solid;
				range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
				range.Style.Border.Left.Style = ExcelBorderStyle.Thick;

				column += decisionTypeWidth;
			} // for each

			row++;

			foreach (string title in groupTitles)
				row = CreateInterestRateSheetSectionGroup(row, lastRawRow, title, cfg);

			return row;
		} // CreateInterestRateSheetSection

		private static readonly List<string> groupTitles = new List<string> {
			InterestRateSheetSectionConfiguration.SetupFeeRate,
			InterestRateSheetSectionConfiguration.SetupFeeAmount,
			InterestRateSheetSectionConfiguration.InterestRate,
		}; 

		private int CreateInterestRateSheetSectionGroup(
			int row,
			int lastRawRow,
			string title,
			InterestRateSheetSectionConfiguration cfg
		) {
			int column = 1;

			ExcelRange range = AStatItem.SetBorders(this.sheet.Cells[row, column]);

			column = range.SetCellValue(title, bIsBold: true, bSetZebra: false, oBgColour: Color.Bisque);
			range.Style.Font.Size = 13;

			// ReSharper disable once UnusedVariable
			foreach (string justForLoop in interestRateSheetDecisionTypes) {
				bool first = true;

				foreach (string colName in interestRateSheetColumns) {
					range = AStatItem.SetBorders(this.sheet.Cells[row, column]);

					column = range.SetCellValue(colName, bIsBold: true, bSetZebra: false, oBgColour: Color.Bisque);
					range.Style.Font.Size = 13;

					if (first)
						range.Style.Border.Left.Style = ExcelBorderStyle.Thick;

					first = false;
				} // for each column
			} // for each decision type

			row++;

			foreach (string scenarioName in this.scenarioNames)
				row = CreateInterestRateSheetSectionRow(row, lastRawRow, title, scenarioName, cfg);

			return row;
		} // CreateInterestRateSheetSectionGroup

		private int CreateInterestRateSheetSectionRow(
			int row,
			int lastRawRow,
			string dataFieldName,
			string scenarioName,
			InterestRateSheetSectionConfiguration cfg
		) {
			int column = 1;

			ExcelRange range = AStatItem.SetBorders(this.sheet.Cells[row, column]);

			column = range.SetCellValue(scenarioName, bIsBold: true, bSetZebra: false);

			foreach (string decisionType in interestRateSheetDecisionTypes) {
				bool first = true;

				foreach (string colName in interestRateSheetColumns) {
					range = AStatItem.SetBorders(this.sheet.Cells[row, column]);

					if (first)
						range.Style.Border.Left.Style = ExcelBorderStyle.Thick;

					first = false;

					switch (colName) {
					case CountInterestColumn:
						CreateCellCount(range, row, lastRawRow, decisionType, cfg);
						break;

					case AvgInterestColumn:
						CreateCellAvg(range, row, lastRawRow, dataFieldName, decisionType, cfg);
						break;

					case WeightAvgInterestColumn:
						CreateCellWeightAvg(range, row, lastRawRow, dataFieldName, decisionType, cfg);
						break;

					case MinInterestColumn:
						CreateCellMin(range, row, lastRawRow, dataFieldName, decisionType, cfg);
						break;

					case MaxInterestColumn:
						CreateCellMax(range, row, lastRawRow, dataFieldName, decisionType, cfg);
						break;
					} // switch

					column++;
				} // for each column
			} // for each decision type

			return row + 1;
		} // CreateInterestRateSheetSectionRow

		private static void CreateCellCount(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.Formula = string.Format(
				"=COUNTIFS({0}!${1}$2:${1}${2}, $A${3}, {0}!${4}$2:${4}${2}, \"{5}\")",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceEscapedCondition
			);

			cell.Style.Numberformat.Format = TitledValue.Format.Int;
		} // CreateCellCount

		private static void CreateCellAvg(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string dataFieldName,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.Formula = string.Format(
				"=AVERAGEIFS({0}!${6}$2:${6}${2}, {0}!${1}$2:${1}${2}, $A${3}, {0}!${4}$2:${4}${2}, \"{5}\")",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceEscapedCondition,
				cfg.DataColumnNames[decisionType][dataFieldName]
			);

			cell.Style.Numberformat.Format = cfg.Formats[dataFieldName];
		} // CreateCellAvg

		private static void CreateCellWeightAvg(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string dataFieldName,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.Formula = string.Format(
				"=SUMPRODUCT(" +
					"({0}!${7}$2:${7}${2} * {0}!${8}$2:${8}${2}) * " +
					"({0}!${1}$2:${1}${2} = $A${3}) * " +
					"({0}!${4}$2:${4}${2}{5})" +
				") / SUMIFS(" +
					"{0}!${8}$2:${8}${2}," +
					"{0}!${1}$2:${1}${2}, $A${3}," +
					"{0}!${4}$2:${4}${2}, \"{6}\"" +
				")",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceCondition,
				cfg.LoanSourceEscapedCondition,
				cfg.DataColumnNames[decisionType][dataFieldName],
				cfg.WeightColumnNames[decisionType]
			);

			cell.Style.Numberformat.Format = cfg.Formats[dataFieldName];
		} // CreateCellWeightAvg

		private static void CreateCellMin(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string dataFieldName,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.CreateArrayFormula(string.Format(
				"=MIN(IF({0}!${1}$2:${1}${2}=$A${3},IF({0}!${4}$2:${4}${2}{5},{0}!${6}$2:${6}${2})))",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceCondition,
				cfg.DataColumnNames[decisionType][dataFieldName]
			));

			cell.Style.Numberformat.Format = cfg.Formats[dataFieldName];
		} // CreateCellWeightMin

		private static void CreateCellMax(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string dataFieldName,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.CreateArrayFormula(string.Format(
				"=MAX(IF({0}!${1}$2:${1}${2}=$A${3},IF({0}!${4}$2:${4}${2}{5},{0}!${6}$2:${6}${2})))",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceCondition,
				cfg.DataColumnNames[decisionType][dataFieldName]
			));

			cell.Style.Numberformat.Format = cfg.Formats[dataFieldName];
		} // CreateCellWeightMax

		private const string CountInterestColumn = "Count";
		private const string AvgInterestColumn = "Simple average";
		private const string WeightAvgInterestColumn = "Weighted average"; 
		private const string MinInterestColumn = "Minimum"; 
		private const string MaxInterestColumn = "Maximum";

		private static readonly List<string> interestRateSheetColumns = new List<string> {
			CountInterestColumn, AvgInterestColumn, WeightAvgInterestColumn, MinInterestColumn, MaxInterestColumn,
		};

		private const string DecisionsSheetName = "Decisions";

		private static readonly List<string> interestRateSheetDecisionTypes = new List<string> {
			InterestRateSheetSectionConfiguration.ManualDecisionType,
			InterestRateSheetSectionConfiguration.MinOfferDecisionType,
			InterestRateSheetSectionConfiguration.MaxOfferDecisionType,
		};

		private readonly ExcelWorksheet sheet;
		private readonly SortedSet<string> scenarioNames; 
	} // class SheetInterestRate
} // namespace
