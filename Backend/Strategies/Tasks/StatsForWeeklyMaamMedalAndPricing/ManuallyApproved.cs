namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class ManuallyApproved : AStatItem {
		public ManuallyApproved(ExcelWorksheet sheet, Total total) : base(
			sheet,
			"Manually approved",
			total
		) {
			this.loanCount = new LoanCount();
		} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(d.Manual(cashRequestIndex).IsApproved, d.Manual(cashRequestIndex).ApprovedAmount))
				this.loanCount += d.Manual(cashRequestIndex).LoanCount;
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("approved / total %", Count, Total.Count),
				new TitledValue("loan count", this.loanCount.Total.Count),
				new TitledValue("default loan count", this.loanCount.Default.Count),
				new TitledValue("bad loan count", this.loanCount.Bad.Count),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("approved amount", Amount),
				new TitledValue("issued amount", this.loanCount.Total.Amount),
				new TitledValue("default amount", this.loanCount.Default.Amount),
				new TitledValue("bad amount", this.loanCount.Bad.Amount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }

		private LoanCount loanCount;
	} // class ManuallyApproved
} // namespace
