namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Drawing;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;

	internal class Stats {
		public Stats(ExcelWorksheet sheet, bool takeMin) {
			this.sheet = sheet;

			var total = new Total(sheet);
			var autoProcessed = new AutoProcessed(sheet, total);
			var autoRejected = new AutoRejected(sheet, total, autoProcessed);
			var autoApproved = new AutoApproved(takeMin, sheet, total, autoProcessed);
			var manuallyRejected = new ManuallyRejected(sheet, total);
			var manuallyApproved = new ManuallyApproved(sheet, total);

			this.stats = new List<AStatItem> {
				total,
				autoProcessed,
				autoRejected,
				new AutoRerejected(sheet, total, autoProcessed, autoRejected),
				autoApproved,
				new AutoReapproved(sheet, total, autoProcessed, autoApproved),
				new NotAutoRejectedAndAutoApproved(sheet, total, autoRejected, autoApproved),
				manuallyRejected,
				new ManuallyAndAutoRejected(sheet, total, manuallyRejected, autoRejected),
				manuallyApproved,
				new ManuallyAndAutoApproved(sheet, total, manuallyApproved, autoApproved),
				new DefaultLoans(sheet, total, manuallyApproved, autoApproved),
				new BadLoans(sheet, total, manuallyApproved, autoApproved),
				new RejectedCrossApproved(sheet, "Manually rejected and auto approved", total, manuallyRejected, autoApproved),
				new RejectedCrossApproved(sheet, "Auto rejected and manually approved", total, autoRejected, manuallyApproved),
			};

			this.name = (takeMin ? "Minimum" : "Maximum") + " offer";
		} // constructor

		public void Add(Datum d) {
			foreach (var si in this.stats)
				si.Add(d);
		} // Add

		// ReSharper disable once UnusedMethodReturnValue.Local
		public int ToXlsx(int row) {
			AStatItem.SetBorder(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, this.name, bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			foreach (var si in this.stats)
				row = si.ToXlsx(row);

			for (int i = 1; i <= AStatItem.LastColumnNumber; i++)
				sheet.Column(i).AutoFit();

			return row;
		} // ToXlsx

		private readonly string name;

		private readonly List<AStatItem> stats; 

		private readonly ExcelWorksheet sheet;
	} // class Stats
} // namespace
