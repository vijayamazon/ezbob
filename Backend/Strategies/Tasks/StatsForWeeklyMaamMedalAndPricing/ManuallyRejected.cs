namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class ManuallyRejected : AStatItem {
		public ManuallyRejected(ExcelWorksheet sheet, Total total) : base(
			sheet,
			"Manually rejected",
			total
		) {} // constructor

		public override void Add(Datum d) {
			Added.If(d.Manual.IsRejected);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("rejected / total %", Count, Total.Count),
			};
		} // PrepareCountRowValues

		private AStatItem Total { get { return Superior[0]; } }
	} // class ManuallyRejected
} // namespace
