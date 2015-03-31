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
			this.defaultLoanAmount= 0;
			this.defaultLoanCount = 0;
			this.badLoanAmount = 0;
			this.badLoanCount = 0;
		} // constructor

		public override void Add(Datum d) {
			if (Added.If(d.FirstManual.IsApproved, d.FirstManual.ApprovedAmount)) {
				this.loanAmount += d.LoanAmount;
				this.loanCount += d.LoanCount;

				this.defaultLoanAmount += d.DefaultLoanAmount;
				this.defaultLoanCount += d.DefaultLoanCount;

				this.badLoanAmount += d.BadLoanAmount;
				this.badLoanCount += d.BadLoanCount;
			} // if
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("approved / total %", Count, Total.Count),
				new TitledValue("loan count", this.loanCount),
				new TitledValue("default loan count", this.defaultLoanCount),
				new TitledValue("bad loan count", this.badLoanCount),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("approved amount", Amount),
				new TitledValue("issued amount", this.loanAmount),
				new TitledValue("default amount", this.defaultLoanAmount),
				new TitledValue("bad amount", this.badLoanAmount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }

		private int loanCount;
		private decimal loanAmount;

		private int defaultLoanCount;
		private decimal defaultLoanAmount;

		private int badLoanCount;
		private decimal badLoanAmount;
	} // class ManuallyApproved
} // namespace
