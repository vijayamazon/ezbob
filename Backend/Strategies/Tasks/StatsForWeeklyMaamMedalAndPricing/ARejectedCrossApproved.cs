namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal abstract class ARejectedCrossApproved : AStatItem {
		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(Rejected.LastWasAdded && Approved.LastWasAdded, Approved.LastAmount))
				LoanCount += d.Manual(cashRequestIndex).ActualLoanCount;
		} // Add

		public abstract int DrawSummary(int row);

		protected ARejectedCrossApproved(
			bool takeMin,
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
			LoanCount = new LoanCount(takeMin, Log);
		} // constructor

		protected override TitledValue[] PrepareCountRowValues() {
			return null;
		} // PrepareCountRowValues

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
					new TitledValue("loan amount", LoanCount.Total.Amount),
					new TitledValue("default issued loan amount", LoanCount.DefaultIssued.Amount),
					new TitledValue("default outstanding loan amount", LoanCount.DefaultOutstanding.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		protected virtual LoanCount LoanCount { get; private set; }

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem Rejected { get { return Superior[1]; } }
		private AStatItem Approved { get { return Superior[2]; } }
	} // class RejectedCrossApproved
} // namespace
