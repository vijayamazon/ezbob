namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Drawing;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class Stats {
		public Stats(ASafeLog log, ExcelWorksheet sheet, bool takeMin, string cashRequestSourceName) {
			this.sheet = sheet;

			log = log.Safe();

			var total = new Total(log, sheet);
			var autoProcessed = new AutoProcessed(log, sheet, total);
			var autoRejected = new AutoRejected(log, sheet, total, autoProcessed);
			var autoApproved = new AutoApproved(log, takeMin, sheet, total, autoProcessed);
			var manuallyRejected = new ManuallyRejected(log, sheet, total);
			ManuallyApproved = new ManuallyApproved(log, sheet, total);

			this.manuallyAndAutoApproved = new ManuallyAndAutoApproved(log, sheet, total, ManuallyApproved, autoApproved);

			this.manuallyRejectedAutoApproved = new ManuallyRejectedAutoApproved(log, sheet, "Manually rejected and auto approved", total, manuallyRejected, autoApproved);
			this.manuallyApprovedAutoRejected = new ManuallyApprovedAutoRejected(log, sheet, "Manually approved and auto rejected", total, autoRejected, ManuallyApproved);

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

			AStatItem.SetBorder(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Summary", bSetZebra: false, oBgColour: Color.Bisque, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 14;
			row++;

			row = this.manuallyAndAutoApproved.DrawSummary(row + 1);

			row = this.manuallyRejectedAutoApproved.DrawSummary(row + 1);

			row = this.manuallyApprovedAutoRejected.DrawSummary(row + 1);

			row++;

			AStatItem.SetBorder(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Details", bSetZebra: false, oBgColour: Color.Bisque, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 14;
			row++;

			foreach (var si in this.stats)
				row = si.ToXlsx(row + 1);

			for (int i = 1; i <= AStatItem.LastColumnNumber; i++)
				this.sheet.Column(i).AutoFit();

			return row;
		} // ToXlsx

		public int FlushLoanIDs(ExcelWorksheet targetSheet, int column) {
			column = FlushLoanIDList(targetSheet, column, "all loans", ManuallyApproved.LoanCount.IDs);

			return FlushLoanIDList(targetSheet, column, "default loans", ManuallyApproved.LoanCount.DefaultIDs);
		} // FlushLoanIDs

		public ManuallyApproved ManuallyApproved { get; private set; }

		private int FlushLoanIDList(ExcelWorksheet targetSheet, int column, string title, SortedSet<int> ids) {
			targetSheet.SetCellValue(1, column, this.name + " - " + title);

			int row = 2;
			foreach (int id in ids) {
				targetSheet.SetCellValue(row, column, id);
				row++;
			} // for each
			
			return column + 1;
		} // FlushLoanIDList

		private readonly string name;

		private readonly List<AStatItem> stats; 

		private readonly ExcelWorksheet sheet;

		private readonly ManuallyAndAutoApproved manuallyAndAutoApproved;
		private readonly ARejectedCrossApproved manuallyRejectedAutoApproved;
		private readonly ARejectedCrossApproved manuallyApprovedAutoRejected;
	} // class Stats
} // namespace
