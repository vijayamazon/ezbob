namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyRejectedAutoApproved : ARejectedCrossApproved {
		public ManuallyRejectedAutoApproved(
			bool takeMin,
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			ManuallyRejected rejected,
			AutoApproved approved
		) : base(
			takeMin,
			log.Safe(),
			sheet,
			title,
			total,
			rejected,
			approved
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(Rejected.LastWasAdded && Approved.LastWasAdded, Approved.LastAmount))
				LoanCount += d.Manual(cashRequestIndex).ActualLoanCount;
		} // Add

		/// <summary>
		/// Issued loan count / approved count.
		/// </summary>
		public decimal IssuedCountRate { get; set; }

		/// <summary>
		/// Default outstanding amount / default issued amount.
		/// </summary>
		public decimal OutstandingAmountRate { get; set; }

		public override int DrawSummary(int row) {
			const decimal defaultIssuedRate = 0.2m;

			int firstRow = row;

			int column = 1;

			column = this.sheet.SetCellValue(row, column, "Manually rejected and auto approved", true);
			column = this.sheet.SetCellValue(row, column, "Manual amount", true);
			column = this.sheet.SetCellValue(row, column, "Manual count", true);
			column = this.sheet.SetCellValue(row, column, "Auto amount", true);
			column = this.sheet.SetCellValue(row, column, "Auto count", true);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Approved", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, Amount, sNumberFormat: TitledValue.Format.Money);
			column = this.sheet.SetCellValue(row, column, Count, sNumberFormat: TitledValue.Format.Int);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Issued", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, Amount * IssuedCountRate, sNumberFormat: TitledValue.Format.Money);
			column = this.sheet.SetCellValue(row, column, Math.Round(Count * IssuedCountRate), sNumberFormat: TitledValue.Format.Int);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default issued", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, Amount * IssuedCountRate * defaultIssuedRate, sNumberFormat: TitledValue.Format.Money);
			column = this.sheet.SetCellValue(row, column, Math.Round(Count * IssuedCountRate * defaultIssuedRate), sNumberFormat: TitledValue.Format.Int);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default issued rate (% of loans)", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, defaultIssuedRate, sNumberFormat: TitledValue.Format.Percent);
			column = this.sheet.SetCellValue(row, column, defaultIssuedRate, sNumberFormat: TitledValue.Format.Percent);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default issued rate (% of approvals)", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default outstanding", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, Amount * IssuedCountRate * defaultIssuedRate * OutstandingAmountRate, sNumberFormat: TitledValue.Format.Money);
			column = this.sheet.SetCellValue(row, column, Math.Round(Count * IssuedCountRate * defaultIssuedRate), sNumberFormat: TitledValue.Format.Int);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default outstanding rate (% of loans)", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default outstanding rate (% of approvals)", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);

			for (int i = firstRow; i <= row; i++)
				for (int j = 1; j <= column; j++)
					SetBorder(this.sheet.Cells[i, j]);

			row++;

			return InsertDivider(row);
		} // DrawSummary

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new [] {
					new TitledValue("count", Count),
				},
				new [] {
					new TitledValue("count / total %", Count, Total.Count),
					new TitledValue("count / rejected %", Count, Rejected.Count),
					new TitledValue("count / approved %", Count, Approved.Count),
				},
				new [] {
					new TitledValue("loan count", LoanCount.Total.Count),
				},
				new [] {
					new TitledValue("default loan count", LoanCount.DefaultIssued.Count),
				},
			};
		} // PrepareMultipleCountRowValues

		protected override List<TitledValue[]> PrepareMultipleAmountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("amount", Amount),
					new TitledValue("amount / approved %", Amount, Approved.Amount),
				},
				new [] {
					new TitledValue("loan amount", LoanCount.Total.Amount),
					new TitledValue("default issued loan amount", LoanCount.DefaultIssued.Amount),
					new TitledValue("default outstanding loan amount", LoanCount.DefaultOutstanding.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private ManuallyRejected Rejected { get { return (ManuallyRejected)Superior[1]; } }
		private AutoApproved Approved { get { return (AutoApproved)Superior[2]; } }
	} // class ManuallyRejectedAutoApproved
} // namespace
