namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel {
	using System.Collections.Generic;
	using System.Drawing;
	using Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;

	internal class SheetApproveFailGroups {
		public SheetApproveFailGroups(ExcelPackage workbook, SheetRawAutomation rawAutomationSheet) {
			this.sheet = workbook.CreateSheet(
				"Auto approve fail groups",
				false,
				"#",
				"Number of reasons",
				"Count",
				"Ratio",
				"Reasons"
			);

			this.rawAutomationSheet = rawAutomationSheet;
		} // constructor

		public void Generate(int lastRawRow) {
			var groups = new List<KeyValuePair<SheetRawAutomation.NonAffirmativeGroupKey, int>>(
				this.rawAutomationSheet.NonAffirmativeGroups
			);

			int totalGroupsCount = this.rawAutomationSheet.NonAffirmativeGroupsCount;

			int row = 2;

			groups.Sort((a, b) => {
				int countOrder = a.Value.CompareTo(b.Value);

				if (countOrder != 0)
					return -countOrder;

				return a.Key.CompareTo(b.Key);
			});

			groups.Add(new KeyValuePair<SheetRawAutomation.NonAffirmativeGroupKey, int>(
				new SheetRawAutomation.NonAffirmativeGroupKey(0),
				totalGroupsCount
			));

			decimal totalCount = totalGroupsCount;

			foreach (KeyValuePair<SheetRawAutomation.NonAffirmativeGroupKey, int> grp in groups) {
				bool isTotal = grp.Key.Hash == "Total";

				int column = 1;

				column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue(
					grp.Key.Hash,
					isTotal,
					!isTotal,
					oBgColour: isTotal ? Color.Bisque : (Color?)null
				);

				column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue(
					grp.Key.Length == 0 ? (int?)null : grp.Key.Length,
					isTotal,
					!isTotal,
					oBgColour: isTotal ? Color.Bisque : (Color?)null,
					sNumberFormat: TitledValue.Format.Int
				);

				column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue(
					grp.Value,
					isTotal,
					!isTotal,
					oBgColour: isTotal ? Color.Bisque : (Color?)null,
					sNumberFormat: TitledValue.Format.Int
				);

				column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue(
					totalGroupsCount == 0 ? 0 : grp.Value / totalCount,
					isTotal,
					!isTotal,
					oBgColour: isTotal ? Color.Bisque : (Color?)null,
					sNumberFormat: TitledValue.Format.Percent
				);

				column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue(
					grp.Key.List,
					isTotal,
					!isTotal,
					oBgColour: isTotal ? Color.Bisque : (Color?)null
				);

				row++;
			} // for each group
		} // Generate

		private readonly ExcelWorksheet sheet;
		private readonly SheetRawAutomation rawAutomationSheet;
	} // class SheetApproveFailGroups
} // namespace
