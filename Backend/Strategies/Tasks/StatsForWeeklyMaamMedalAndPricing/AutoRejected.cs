namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class AutoRejected : AStatItem {
		public AutoRejected(ExcelWorksheet sheet, bool takeLast, Total total, AutoProcessed autoProcessed) : base(
			sheet,
			"Auto rejected",
			total,
			autoProcessed
		) {
			this.takeLast = takeLast;
		} // constructor

		public override void Add(Datum d) {
			Added.If(
				d.Auto(this.takeLast).HasDecided &&
				d.Auto(this.takeLast).AutomationDecision.In(DecisionActions.Reject, DecisionActions.ReReject)
			);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("rejected / total %", Count, Total.Count),
				new TitledValue("rejected / processed %", Count, AutoProcessed.Count),
			};
		} // PrepareCountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem AutoProcessed { get { return Superior[1]; } }

		private readonly bool takeLast;
	} // class AutoRejected
} // namespace
