﻿namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class DefaultLoans : AStatItem {
		public DefaultLoans(
			ExcelWorksheet sheet,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved
		) : base(
			sheet,
			"Default (and remained) loans",
			total,
			manuallyApproved,
			autoApproved
		) {
		} // constructor

		public override void Add(Datum d) {
			if (Added.If(ManuallyApproved.LastWasAdded && AutoApproved.LastWasAdded && d.HasDefaultLoan)) {
				this.approvedAmount += AutoApproved.LastAmount;
				this.loanAmount += d.DefaultLoanAmount;
			} // if
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("count / total %", Count, Total.Count),
				new TitledValue("count / manually approved count %", Count, ManuallyApproved.Count),
				new TitledValue("count / auto approved count %", Count, AutoApproved.Count),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("approved amount", this.approvedAmount),
				new TitledValue("approved amount / manually approved amount %", this.approvedAmount, ManuallyApproved.Amount),
				new TitledValue("approved amount / auto approved amount %", this.approvedAmount, AutoApproved.Amount),
				new TitledValue("loan amount", this.loanAmount),
				new TitledValue("loan amount / manually approved amount %", this.loanAmount, ManuallyApproved.Amount),
				new TitledValue("loan amount / auto approved amount %", this.loanAmount, AutoApproved.Amount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem ManuallyApproved { get { return Superior[1]; } }
		private AStatItem AutoApproved { get { return Superior[2]; } }

		private decimal approvedAmount;
		private decimal loanAmount;
	} // class DefaultLoans
} // namespace
