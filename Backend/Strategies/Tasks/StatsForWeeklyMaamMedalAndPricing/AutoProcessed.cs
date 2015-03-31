namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class AutoProcessed : AStatItem {
		public AutoProcessed(ExcelWorksheet sheet, bool takeLast, Total total) : base(
			sheet,
			"Auto processed",
			total
		) {
			this.takeLast = takeLast;
		} // constructor

		public override void Add(Datum d) {
			Added.If(d.Auto(this.takeLast).HasDecided);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("processed / total %", Count, Superior[0].Count),
			};
		} // PrepareCountRowValues

		private readonly bool takeLast;
	} // class AutoProcessed
} // namespace
