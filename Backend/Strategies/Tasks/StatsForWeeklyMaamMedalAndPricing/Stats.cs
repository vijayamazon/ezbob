namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Drawing;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;

	internal class Stats {
		public Stats(ExcelWorksheet sheet, bool takeMin, string cashRequestSourceName) {
			this.sheet = sheet;

			var total = new Total(sheet);
			var autoProcessed = new AutoProcessed(sheet, total);
			var autoRejected = new AutoRejected(sheet, total, autoProcessed);
			var autoApproved = new AutoApproved(takeMin, sheet, total, autoProcessed);
			var manuallyRejected = new ManuallyRejected(sheet, total);
			var manuallyApproved = new ManuallyApproved(sheet, total);
			var defaultLoans = new DefaultLoans(sheet, total, manuallyApproved, autoApproved);

			this.manuallyAndAutoApproved = new ManuallyAndAutoApproved(sheet, total, manuallyApproved, autoApproved, defaultLoans);

			this.manuallyRejectedAutoApproved = new ManuallyRejectedAutoApproved(sheet, "Manually rejected and auto approved", total, manuallyRejected, autoApproved);
			this.manuallyApprovedAutoRejected = new ManuallyApprovedAutoRejected(sheet, "Manually approved and auto rejected", total, autoRejected, manuallyApproved);

			this.stats = new List<AStatItem> {
				total,
				autoProcessed,
				autoRejected,
				autoApproved,
				manuallyRejected,
				new ManuallyAndAutoRejected(sheet, total, manuallyRejected, autoRejected),
				manuallyApproved,
				defaultLoans,
				new BadLoans(sheet, total, manuallyApproved, autoApproved),
				this.manuallyAndAutoApproved,
				this.manuallyRejectedAutoApproved,
				this.manuallyApprovedAutoRejected,
			};

			this.name = (takeMin ? "Minimum" : "Maximum") + " offer " + cashRequestSourceName;
		} // constructor

		public void Add(Datum d, int cashRequstIndex) {
			foreach (AStatItem si in this.stats)
				si.Add(d, cashRequstIndex);
		} // Add

		// ReSharper disable once UnusedMethodReturnValue.Local
		public int ToXlsx(int row) {
			AStatItem.SetBorder(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, this.name, bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			foreach (var si in this.stats)
				row = si.ToXlsx(row);

			row++;
			AStatItem.SetBorder(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Summary", bSetZebra: false, oBgColour: Color.Bisque, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 14;
			row++;

			row = manuallyAndAutoApproved.DrawSummary(row + 1);

			row = manuallyRejectedAutoApproved.DrawSummary(row + 1);

			row = manuallyApprovedAutoRejected.DrawSummary(row + 1);

			for (int i = 1; i <= AStatItem.LastColumnNumber; i++)
				sheet.Column(i).AutoFit();

			return row;
		} // ToXlsx

		private readonly string name;

		private readonly List<AStatItem> stats; 

		private readonly ExcelWorksheet sheet;

		private readonly ManuallyAndAutoApproved manuallyAndAutoApproved;
		private readonly ARejectedCrossApproved manuallyRejectedAutoApproved;
		private readonly ARejectedCrossApproved manuallyApprovedAutoRejected;
	} // class Stats
} // namespace
