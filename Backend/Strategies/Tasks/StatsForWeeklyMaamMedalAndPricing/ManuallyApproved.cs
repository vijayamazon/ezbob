namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class ManuallyApproved : AStatItem {
		public ManuallyApproved(ExcelWorksheet sheet, Total total) : base(
			sheet,
			"Manually approved",
			total
		) {} // constructor

		public override void Add(Datum d) {
			Added.If(d.FirstManual.IsApproved, d.FirstManual.ApprovedAmount);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("approved / total %", Count, Total.Count),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("amount", Amount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
	} // class ManuallyApproved
} // namespace
