namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyApprovedAutoRejected : ARejectedCrossApproved {
		public ManuallyApprovedAutoRejected(
			bool takeMin,
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			AStatItem rejected,
			AStatItem approved
		) : base(
			takeMin,
			log.Safe(),
			sheet,
			title,
			total,
			rejected,
			approved
		) {} // constructor

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
	} // class ManuallyApprovedAutoRejected
} // namespace
