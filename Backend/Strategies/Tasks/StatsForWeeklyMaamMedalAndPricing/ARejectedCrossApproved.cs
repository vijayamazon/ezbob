namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal abstract class ARejectedCrossApproved : AStatItem {
		public override void Add(Datum d) {
			if (Added.If(Rejected.LastWasAdded && Approved.LastWasAdded, Approved.LastAmount)) {
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

			row = SetRowValues(row, "Approved", OrderApprovedAndRejected(
				new TitledValue(0, 0),
				new TitledValue(Amount, Count)
			));

			row = SetRowValues(row, "Issued", OrderApprovedAndRejected(
				new TitledValue(0, 0),
				new TitledValue(this.loanAmount, this.loanCount)
			));

			row = SetRowValues(row, "Default", OrderApprovedAndRejected(
				new TitledValue(0, 0),
				new TitledValue(this.defaultLoanAmount, this.defaultLoanCount)
			));

			row = SetRowValues(row, "Default rate (% of loans)", OrderApprovedAndRejected(
				new TitledValue(0, 0),
				new TitledValue(this.defaultLoanAmount, this.loanAmount, this.defaultLoanCount, this.loanCount)
			));

			row = SetRowValues(row, "Default rate (% of approvals)", OrderApprovedAndRejected(
				new TitledValue(0, 0),
				new TitledValue(this.defaultLoanAmount, Amount, this.defaultLoanCount, Count)
			));

			return row;
		} // DrawSummary

		protected ARejectedCrossApproved(
			ExcelWorksheet sheet,
			string title,
			Total total,
			AStatItem rejected,
			AStatItem approved
		) : base(
			sheet,
			title,
			total,
			rejected,
			approved
		) {
			this.loanCount = 0;
			this.badLoanCount = 0;
			this.defaultLoanCount = 0;

			this.loanAmount = 0;
			this.badLoanAmount = 0;
			this.defaultLoanAmount = 0;
		} // constructor

		protected override TitledValue[] PrepareCountRowValues() {
			return null;
		} // PrepareCountRowValues

		protected abstract TitledValue[] OrderApprovedAndRejected(TitledValue rejected, TitledValue approved);

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new [] {
					new TitledValue("count", Count),
					new TitledValue("count / total %", Count, Total.Count),
					new TitledValue("count / rejected %", Count, Rejected.Count),
					new TitledValue("count / approved %", Count, Approved.Count),
				},
				new [] {
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
					new TitledValue("amount", Amount),
					new TitledValue("amount / approved %", Amount, Approved.Amount),
				},
				new [] {
					new TitledValue("loan amount", this.loanAmount),
					new TitledValue("default loan amount", this.defaultLoanAmount),
					new TitledValue("bad loan amount", this.badLoanAmount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem Rejected { get { return Superior[1]; } }
		private AStatItem Approved { get { return Superior[2]; } }

		private int loanCount;
		private int badLoanCount;
		private int defaultLoanCount;

		private decimal loanAmount;
		private decimal badLoanAmount;
		private decimal defaultLoanAmount;
	} // class RejectedCrossApproved
} // namespace
