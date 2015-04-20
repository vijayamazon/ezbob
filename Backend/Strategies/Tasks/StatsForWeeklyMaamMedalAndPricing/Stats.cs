namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Drawing;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	internal class Stats {
		public Stats(
			ASafeLog log,
			ExcelWorksheet sheet,
			bool takeMin,
			string summaryTableFormulaPattern,
			Color titleBgColour
		) {
			this.sheet = sheet;
			this.titleBgColour = titleBgColour;

			log = log.Safe();

			var total = new Total(log, sheet);
			var autoProcessed = new AutoProcessed(log, sheet, total);
			var autoRejected = new AutoRejected(log, sheet, total, autoProcessed);
			var autoApproved = new AutoApproved(log, takeMin, sheet, total, autoProcessed);
			var manuallyRejected = new ManuallyRejected(log, sheet, total);
			ManuallyApproved = new ManuallyApproved(log, sheet, total);

			this.stats = new List<AStatItem> {
				total,
				autoProcessed,
				autoRejected,
				autoApproved,
				manuallyRejected,
				new ManuallyAndAutoRejected(log, sheet, total, manuallyRejected, autoRejected),
				ManuallyApproved,
				new DefaultIssuedLoans(log, sheet, total, ManuallyApproved, autoApproved),
				new DefaultOutstandingLoans(log, sheet, total, ManuallyApproved, autoApproved),
				new ManuallyAndAutoApproved(takeMin, log, sheet, total, ManuallyApproved, autoApproved),
				new ManuallyRejectedAutoApproved(takeMin, log, sheet, "Manually rejected and auto approved", total, manuallyRejected, autoApproved),
				new ManuallyApprovedAutoNotApproved(takeMin, log, sheet, "Manually approved and auto NOT approved", total, ManuallyApproved, autoApproved),
			};

			this.name = (takeMin ? "Minimum" : "Maximum") + " offer";

			this.summaryTableFormulaPattern = summaryTableFormulaPattern;
		} // constructor

		public void Add(Datum d, int cashRequstIndex) {
			foreach (AStatItem si in this.stats)
				si.Add(d, cashRequstIndex);
		} // Add

		// ReSharper disable once UnusedMethodReturnValue.Local
		public int ToXlsx(int row, int lastRawRow) {
			row = DrawSummary(row, lastRawRow);

			/*
			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Details", bSetZebra: false, oBgColour: Color.Coral, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 14;
			row++;

			foreach (var si in this.stats)
				row = si.ToXlsx(row + 1);

			for (int i = 1; i <= AStatItem.LastColumnNumber; i++)
				this.sheet.Column(i).AutoFit();
			*/

			return row;
		} // ToXlsx

		public int FlushLoanIDs(ExcelWorksheet targetSheet, int column) {
			column = FlushLoanIDList(targetSheet, column, "all loans", ManuallyApproved.LoanCount.IDs);

			return FlushLoanIDList(targetSheet, column, "default loans", ManuallyApproved.LoanCount.DefaultIDs);
		} // FlushLoanIDs

		public ManuallyApproved ManuallyApproved { get; private set; }

		private int FlushLoanIDList(ExcelWorksheet targetSheet, int column, string title, IEnumerable<int> ids) {
			targetSheet.SetCellValue(1, column, this.name + " - " + title);

			int row = 2;
			foreach (int id in ids) {
				targetSheet.SetCellValue(row, column, id);
				row++;
			} // for each
			
			return column + 1;
		} // FlushLoanIDList

		private int DrawSummary(int row, int lastRawRow) {
			string[] lines = this.summaryTableFormulaPattern.Split('\n');

			string[] titles = lines[0].Split('\t');

			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, 5]).Merge = true;
			AStatItem.SetBorders(this.sheet.Cells[row, 6, row, 9]).Merge = true;
			AStatItem.SetBorders(this.sheet.Cells[row, 10, row, 13]).Merge = true;

			for (int i = 0; i < titles.Length; i++) {
				ExcelRange range = this.sheet.Cells[row, i + 1];

				range.SetCellValue(null, bSetZebra: false, oBgColour: this.titleBgColour, bIsBold: true);
				range.Style.Font.Size = 16;

				string title = titles[i];

				if (!string.IsNullOrWhiteSpace(title)) {
					range.Formula = title.Trim();

					if (i > 0)
						range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
				} // if
			} // for

			row++;

			for (int i = 1; i < lines.Length; i++) {
				string line = lines[i];

				if (string.IsNullOrWhiteSpace(line))
					continue;

				string[] values = line.Replace("__LAST_RAW_ROW__", lastRawRow.ToString()).Split('\t');

				string rowTitle = values[0];

				bool localFormula =
					rowTitle.StartsWith("Average") ||
					rowTitle.StartsWith("Issued amount") ||
					rowTitle.StartsWith("Default issued rate") ||
					rowTitle.StartsWith("Default outstanding rate");

				string amountFormat;
				string countFormat = null;

				if (localFormula) {
					if (rowTitle.StartsWith("Average")) {
						amountFormat = TitledValue.Format.Money;
					} else {
						amountFormat = TitledValue.Format.Percent;
						countFormat = TitledValue.Format.Percent;
					} // if
				} else {
					amountFormat = TitledValue.Format.Money;
					countFormat = TitledValue.Format.Int;
				} // if

				bool sectionStart = rowTitle.StartsWith("Manually") || (rowTitle == "TOTAL");

				for (int j = 0; j < 13; j++) {
					string formula = values[j].Trim();

					bool boldFont = sectionStart || (j == 0);

					Color fontColor = Color.Black;

					var cell = AStatItem.SetBorders(this.sheet.Cells[row, j + 1]);

					if (!sectionStart)
						fontColor = localFormula ? Color.DarkOrchid : Color.Red;

					if (formula.StartsWith("=")) {
						cell.Formula = values[j].Trim();

						if (!localFormula)
							boldFont = true;
					} else {
						if (formula == "N/A")
							fontColor = Color.LightGray;

						cell.Value = formula;
					} // if

					cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
					cell.Style.Fill.BackgroundColor.SetColor(sectionStart ? Color.Bisque : Color.White);
					cell.Style.Font.Bold = boldFont;
					cell.Style.Font.Color.SetColor(fontColor);

					int mod = (j - 1) % 4;

					switch (mod) {
					case 0:
						cell.Style.Border.Left.Style = ExcelBorderStyle.Thick;
						goto case 2;

					case 2:
						cell.Style.Numberformat.Format = amountFormat;
						break;

					case 1:
					case 3:
						if (countFormat != null)
							cell.Style.Numberformat.Format = countFormat;

						break;
					} // switch
				} // for each formula in row

				row++;
			} // for each row

			return row + 1;
		} // DrawSummary

		private readonly string name;
		private readonly List<AStatItem> stats; 
		private readonly ExcelWorksheet sheet;
		private readonly string summaryTableFormulaPattern;
		private readonly Color titleBgColour;
	} // class Stats
} // namespace
