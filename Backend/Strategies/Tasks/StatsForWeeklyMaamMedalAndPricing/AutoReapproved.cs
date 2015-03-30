namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class AutoReapproved : AStatItem {
		public AutoReapproved(
			ExcelWorksheet sheet,
			Total total,
			AutoProcessed autoProcessed,
			AutoApproved autoApproved
		) : base(
			sheet,
			"Auto re-approved",
			total,
			autoProcessed,
			autoApproved
		) {} // constructor

		public override void Add(Datum d) {
			// TODO Added.If(d.AutomationDecision == DecisionActions.ReApprove);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("re-approved / total %", Count, Total.Count),
				new TitledValue("re-approved / processed %", Count, AutoProcessed.Count),
				new TitledValue("re-approved / approved %", Count, AutoApproved.Count),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("amount", Amount),
				new TitledValue("re-approved / approved %", Amount, AutoApproved.Amount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem AutoProcessed { get { return Superior[1]; } }
		private AStatItem AutoApproved { get { return Superior[2]; } }
	} // class AutoReapproved
} // namespace
