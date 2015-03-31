namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class ManuallyApproved : AStatItem {
		public ManuallyApproved(ExcelWorksheet sheet, Total total) : base(
			sheet,
			"Manually approved",
			total
		) {
			this.loanAmount = 0;
			this.loanCount = 0;
		} // constructor

		public override void Add(Datum d) {
			if (Added.If(d.FirstManual.IsApproved, d.FirstManual.ApprovedAmount)) {
				this.loanAmount += d.LoanAmount;
				this.loanCount += d.LoanCount;
			} // if
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("approved / total %", Count, Total.Count),
				new TitledValue("loan count", this.loanCount),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("approved amount", Amount),
				new TitledValue("issued amount", this.loanAmount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }

		private int loanCount;
		private decimal loanAmount;
	} // class ManuallyApproved
} // namespace
