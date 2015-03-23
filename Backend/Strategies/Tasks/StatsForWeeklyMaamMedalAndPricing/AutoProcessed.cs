namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class AutoProcessed : AStatItem {
		public AutoProcessed(ExcelWorksheet sheet, Total total) : base(
			sheet,
			"Auto processed",
			total
		) {} // constructor

		public override void Add(Datum d) {
			Added.If(d.AutomationDecision != DecisionActions.Waiting);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("processed / total %", Count, Superior[0].Count),
			};
		} // PrepareCountRowValues
	} // class AutoProcessed
} // namespace
