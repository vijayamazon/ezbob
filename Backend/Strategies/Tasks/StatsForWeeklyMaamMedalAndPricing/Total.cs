namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class Total : AStatItem {
		public Total(ExcelWorksheet sheet) : base(sheet, "Total") {
			this.cashRequestCount = 0;
		} // constructor

		public override void Add(Datum d) {
			Added.Yes();
			this.cashRequestCount += d.ManualItems.Count;
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("cash request count", this.cashRequestCount),
			};
		} // PrepareCountRowValues

		private int cashRequestCount;
	} // class Total
} // namespace
