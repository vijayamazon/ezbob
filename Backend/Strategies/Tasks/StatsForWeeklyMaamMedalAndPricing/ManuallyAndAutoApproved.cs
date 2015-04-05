namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyAndAutoApproved : AStatItem {
		public ManuallyAndAutoApproved(
			ASafeLog log,
			ExcelWorksheet sheet,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved,
			DefaultLoans defaultLoans
		) : base(
			log.Safe(),
			sheet,
			"Manually and auto approved",
			total,
			manuallyApproved,
			autoApproved,
			defaultLoans
		) {
			this.manualAmount = 0;
			this.autoAmount = 0;

			this.loanCount = new LoanCount(Log);
		} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(ManuallyApproved.LastWasAdded && AutoApproved.LastWasAdded)) {
				this.manualAmount += ManuallyApproved.LastAmount;
				this.autoAmount += AutoApproved.LastAmount;

				this.loanCount += d.Manual(cashRequestIndex).LoanCount;
			} // if
		} // Add

		public int DrawSummary(int row) {
			row = SetRowValues(row, true,
				new TitledValue("Manual amount", "Manual count"),
				new TitledValue("Auto amount", "Auto count")
			);

			row = SetRowValues(row, "Approved",
				new TitledValue(this.manualAmount, Count),
				new TitledValue(this.autoAmount, Count)
			);

			var autoIssued = this.manualAmount == 0 ? 0 : this.loanCount.Total.Amount / this.manualAmount * this.autoAmount;

			row = SetRowValues(row, "Issued",
				new TitledValue(this.loanCount.Total.Amount, this.loanCount.Total.Count),
				new TitledValue(autoIssued, this.loanCount.Total.Amount)
			);

			row = SetRowValues(row, "Default",
				new TitledValue(this.loanCount.Default.Amount, this.loanCount.Default.Count),
				new TitledValue(DefaultLoans.Amount, DefaultLoans.Count)
			);

			row = SetRowValues(row, "Default rate (% of loans)",
				new TitledValue(
					this.loanCount.Default.Amount, this.loanCount.Total.Amount,
					this.loanCount.Default.Count, this.loanCount.Total.Count
				),
				new TitledValue(
					DefaultLoans.Amount, autoIssued,
					DefaultLoans.Count, this.loanCount.Total.Count
				)
			);

			row = SetRowValues(row, "Default rate (% of approvals)",
				new TitledValue(
					this.loanCount.Default.Amount, this.manualAmount,
					this.loanCount.Default.Count, Count
				),
				new TitledValue(DefaultLoans.Amount, this.autoAmount, DefaultLoans.Count, Count)
			);

			return row;
		} // DrawSummary

		protected override TitledValue[] PrepareCountRowValues() {
			return null;
		} // PrepareCountRowValues

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("count", Count),
					new TitledValue("both approved / total %", Count, Total.Count),
					new TitledValue("both approved / manually approved %", Count, ManuallyApproved.Count),
					new TitledValue("both approved / autoApproved %", Count, AutoApproved.Count),
				},
				new[] {
					new TitledValue("loan count", this.loanCount.Total.Count),
					new TitledValue("default loan count", this.loanCount.Default.Count),
					new TitledValue("bad loan count", this.loanCount.Bad.Count),
				},
			};
		} // PrepareMultipleCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return null;
		} // PrepareAmountRowValues

		protected override List<TitledValue[]> PrepareMultipleAmountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("manual amount", this.manualAmount),
					new TitledValue("manual amount / total manual amount %", this.manualAmount, ManuallyApproved.Amount),
					new TitledValue("auto amount", this.autoAmount),
					new TitledValue("auto amount / totalAuto amount %", this.autoAmount, AutoApproved.Amount),
					new TitledValue("auto amount / manual amount %", this.autoAmount, this.manualAmount),
				},
				new[] {
					new TitledValue("loan amount", this.loanCount.Total.Amount),
					new TitledValue("default loan amount", this.loanCount.Default.Amount),
					new TitledValue("bad loan amount", this.loanCount.Bad.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem ManuallyApproved { get { return Superior[1]; } }
		private AStatItem AutoApproved { get { return Superior[2]; } }
		private AStatItem DefaultLoans { get { return Superior[3]; } }

		private decimal manualAmount;
		private decimal autoAmount;

		private LoanCount loanCount;
	} // class ManuallyAndAutoApproved
} // namespace
