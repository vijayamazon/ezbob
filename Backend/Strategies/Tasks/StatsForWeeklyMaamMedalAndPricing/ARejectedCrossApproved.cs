namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal abstract class ARejectedCrossApproved : AStatItem {
		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(Rejected.LastWasAdded && Approved.LastWasAdded, Approved.LastAmount))
				this.loanCount += d.Manual(cashRequestIndex).LoanCount;
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
				new TitledValue(this.loanCount.Total.Amount, this.loanCount.Total.Count)
			));

			row = SetRowValues(row, "Default", OrderApprovedAndRejected(
				new TitledValue(0, 0),
				new TitledValue(this.loanCount.Default.Amount, this.loanCount.Default.Count)
			));

			row = SetRowValues(row, "Default rate (% of loans)", OrderApprovedAndRejected(
				new TitledValue(0, 0),
				new TitledValue(
					this.loanCount.Default.Amount, this.loanCount.Total.Amount,
					this.loanCount.Default.Count, this.loanCount.Total.Count
				)
			));

			row = SetRowValues(row, "Default rate (% of approvals)", OrderApprovedAndRejected(
				new TitledValue(0, 0),
				new TitledValue(this.loanCount.Default.Amount, Amount, this.loanCount.Default.Count, Count)
			));

			return row;
		} // DrawSummary

		protected ARejectedCrossApproved(
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			AStatItem rejected,
			AStatItem approved
		) : base(
			log,
			sheet,
			title,
			total,
			rejected,
			approved
		) {
			this.loanCount = new LoanCount(Log);
		} // constructor

		protected override TitledValue[] PrepareCountRowValues() {
			return null;
		} // PrepareCountRowValues

		protected abstract TitledValue[] OrderApprovedAndRejected(TitledValue rejected, TitledValue approved);

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
					new TitledValue("loan count", this.loanCount.Total.Count),
				},
				new [] {
					new TitledValue("default loan count", this.loanCount.Default.Count),
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
					new TitledValue("loan amount", this.loanCount.Total.Amount),
					new TitledValue("default loan amount", this.loanCount.Default.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem Rejected { get { return Superior[1]; } }
		private AStatItem Approved { get { return Superior[2]; } }

		private LoanCount loanCount;
	} // class RejectedCrossApproved
} // namespace
