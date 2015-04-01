namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class Total : AStatItem {
		public Total(ExcelWorksheet sheet) : base(sheet, "Total") {
		} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			Added.Yes();
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
			};
		} // PrepareCountRowValues
	} // class Total
} // namespace
