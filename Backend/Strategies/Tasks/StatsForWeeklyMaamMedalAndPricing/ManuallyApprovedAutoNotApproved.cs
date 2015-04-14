namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyApprovedAutoNotApproved : ARejectedCrossApproved {
		public ManuallyApprovedAutoNotApproved(
			bool takeMin,
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved
		) : base(
			takeMin,
			log.Safe(),
			sheet,
			title,
			total,
			manuallyApproved,
			autoApproved
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(ManuallyApproved.LastWasAdded && !AutoApproved.LastWasAdded, ManuallyApproved.LastAmount))
				LoanCount += d.Manual(cashRequestIndex).ActualLoanCount;
		} // Add

		public override int DrawSummary(int row) {
			row = SetRowValues(row, true,
				new TitledValue("Manual amount", "Manual count"),
				new TitledValue("Auto amount", "Auto count")
			);

			row = SetRowValues(row, "Approved",
				new TitledValue(Amount, Count),
				new TitledValue(0, 0)
			);

			row = SetRowValues(row, "Issued",
				new TitledValue(LoanCount.Total.Amount, LoanCount.Total.Count),
				new TitledValue(0, 0)
			);

			row = SetRowValues(row, "Default issued",
				new TitledValue(LoanCount.DefaultIssued.Amount, LoanCount.DefaultIssued.Count),
				new TitledValue(0, 0)
			);

			row = SetRowValues(row, "Default issued rate (% of loans)",
				new TitledValue(
					LoanCount.DefaultIssued.Amount, LoanCount.Total.Amount,
					LoanCount.DefaultIssued.Count, LoanCount.Total.Count
				),
				new TitledValue(0, 0)
			);

			row = SetRowValues(row, "Default issued rate (% of approvals)",
				new TitledValue(LoanCount.DefaultIssued.Amount, Amount, LoanCount.DefaultIssued.Count, Count),
				new TitledValue(0, 0)
			);

			row = SetRowValues(row, "Default outstanding",
				new TitledValue(0, 0),
				new TitledValue(LoanCount.DefaultOutstanding.Amount, LoanCount.DefaultOutstanding.Count)
			);

			row = SetRowValues(row, "Default outstanding rate (% of loans)",
				new TitledValue(
					LoanCount.DefaultOutstanding.Amount, LoanCount.Total.Amount,
					LoanCount.DefaultOutstanding.Count, LoanCount.Total.Count
				),
				new TitledValue(0, 0)
			);

			row = SetRowValues(row, "Default outstanding rate (% of approvals)",
				new TitledValue(
					LoanCount.DefaultOutstanding.Amount, Amount,
					LoanCount.DefaultOutstanding.Count, Count
				),
				new TitledValue(0, 0)
			);

			return InsertDivider(row);
		} // DrawSummary

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new [] {
					new TitledValue("count", Count),
				},
				new [] {
					new TitledValue("count / total %", Count, Total.Count),
					new TitledValue("count / not auto approved %", Count, Total.Count - AutoApproved.Count),
					new TitledValue("count / manually approved %", Count, ManuallyApproved.Count),
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
					new TitledValue("amount / manually approved %", Amount, ManuallyApproved.Amount),
				},
				new [] {
					new TitledValue("loan amount", LoanCount.Total.Amount),
					new TitledValue("default issued loan amount", LoanCount.DefaultIssued.Amount),
					new TitledValue("default outstanding loan amount", LoanCount.DefaultOutstanding.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private ManuallyApproved ManuallyApproved { get { return (ManuallyApproved)Superior[1]; } }
		private AutoApproved AutoApproved { get { return (AutoApproved)Superior[2]; } }
	} // class ManuallyApprovedAutoNotApproved
} // namespace
