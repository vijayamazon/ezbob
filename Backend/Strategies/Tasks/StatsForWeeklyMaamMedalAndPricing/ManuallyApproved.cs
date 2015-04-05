namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyApproved : AStatItem {
		public ManuallyApproved(ASafeLog log, ExcelWorksheet sheet, Total total) : base(
			log.Safe(),
			sheet,
			"Manually approved",
			total
		) {
			LoanCount = new LoanCount(Log);
		} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(d.Manual(cashRequestIndex).IsApproved, d.Manual(cashRequestIndex).ApprovedAmount))
				LoanCount += d.Manual(cashRequestIndex).LoanCount;
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("approved / total %", Count, Total.Count),
				new TitledValue("loan count", LoanCount.Total.Count),
				new TitledValue("default loan count", LoanCount.Default.Count),
				new TitledValue("bad loan count", LoanCount.Bad.Count),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("approved amount", Amount),
				new TitledValue("issued amount", LoanCount.Total.Amount),
				new TitledValue("default amount", LoanCount.Default.Amount),
				new TitledValue("bad amount", LoanCount.Bad.Amount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }

		public LoanCount LoanCount { get; private set; }
	} // class ManuallyApproved
} // namespace
