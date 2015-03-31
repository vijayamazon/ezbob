namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class ManuallyAndAutoApproved : AStatItem {
		public ManuallyAndAutoApproved(
			ExcelWorksheet sheet,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved,
			DefaultLoans defaultLoans
		) : base(
			sheet,
			"Manually and auto approved",
			total,
			manuallyApproved,
			autoApproved,
			defaultLoans
		) {
			this.manualAmount = 0;
			this.autoAmount = 0;

			this.loanCount = 0;
			this.badLoanCount = 0;
			this.defaultLoanCount = 0;

			this.loanAmount = 0;
			this.badLoanAmount = 0;
			this.defaultLoanAmount = 0;
		} // constructor

		public override void Add(Datum d) {
			if (Added.If(ManuallyApproved.LastWasAdded && AutoApproved.LastWasAdded)) {
				this.manualAmount += ManuallyApproved.LastAmount;
				this.autoAmount += AutoApproved.LastAmount;

				this.loanCount += d.LoanCount;
				this.badLoanCount += d.BadLoanCount;
				this.defaultLoanCount += d.DefaultLoanCount;

				this.loanAmount += d.LoanAmount;
				this.badLoanAmount += d.BadLoanAmount;
				this.defaultLoanAmount += d.DefaultLoanAmount;
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

			var autoIssued = this.manualAmount == 0 ? 0 : this.loanAmount / this.manualAmount * this.autoAmount;

			row = SetRowValues(row, "Issued",
				new TitledValue(this.loanAmount, this.loanCount),
				new TitledValue(autoIssued, this.loanCount)
			);

			row = SetRowValues(row, "Default",
				new TitledValue(this.defaultLoanAmount, this.defaultLoanCount),
				new TitledValue(DefaultLoans.Amount, DefaultLoans.Count)
			);

			row = SetRowValues(row, "Default rate (% of loans)",
				new TitledValue(this.defaultLoanAmount, this.loanAmount, this.defaultLoanCount, this.loanCount),
				new TitledValue(DefaultLoans.Amount, autoIssued, DefaultLoans.Count, this.loanCount)
			);

			row = SetRowValues(row, "Default rate (% of approvals)",
				new TitledValue(this.defaultLoanAmount, this.manualAmount, this.defaultLoanCount, Count),
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
					new TitledValue("loan count", this.loanCount),
					new TitledValue("default loan count", this.defaultLoanCount),
					new TitledValue("bad loan count", this.badLoanCount),
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
					new TitledValue("loan amount", this.loanAmount),
					new TitledValue("default loan amount", this.defaultLoanAmount),
					new TitledValue("bad loan amount", this.badLoanAmount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem ManuallyApproved { get { return Superior[1]; } }
		private AStatItem AutoApproved { get { return Superior[2]; } }
		private AStatItem DefaultLoans { get { return Superior[3]; } }

		private decimal manualAmount;
		private decimal autoAmount;

		private int loanCount;
		private int badLoanCount;
		private int defaultLoanCount;

		private decimal loanAmount;
		private decimal badLoanAmount;
		private decimal defaultLoanAmount;
	} // class ManuallyAndAutoApproved
} // namespace
