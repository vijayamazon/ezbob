namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class AutoApproved : AStatItem {
		public AutoApproved(bool takeMin, ExcelWorksheet sheet, Total total, AutoProcessed autoProcessed) : base(
			sheet,
			"Auto approved & re-approved",
			total,
			autoProcessed
		) {
			this.takeMin = takeMin;
		} // constructor

		public override void Add(Datum d) {
			Added.If(
				(d.AutomationDecision == DecisionActions.ReApprove) ||
				(this.takeMin ? (d.AutomationDecision == DecisionActions.Approve) : (d.AutoMaxOrMin.Amount > 0)),
				this.takeMin ? d.AutoMin.Amount : d.AutoMaxOrMin.Amount
			);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("approved / total %", Count, Total.Count),
				new TitledValue("approved / processed %", Count, AutoProcessed.Count),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("amount", Amount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem AutoProcessed { get { return Superior[1]; } }

		private readonly bool takeMin;
	} // class AutoApproved
} // namespace
