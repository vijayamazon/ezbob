namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class AutoApproved : AStatItem {
		public AutoApproved(
			ASafeLog log,
			bool takeMin,
			ExcelWorksheet sheet,
			Total total,
			AutoProcessed autoProcessed
		) : base(
			log,
			sheet,
			"Auto approved",
			total,
			autoProcessed
		) {
			this.takeMin = takeMin;
		} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			AutoDatumItem auto = d.Auto(cashRequestIndex);

			Added.If(
				auto.HasDecided && auto.AutomationDecision.In(DecisionActions.Approve, DecisionActions.ReApprove),
				auto.AutomationDecision == DecisionActions.ReApprove ? auto.ReapprovedAmount : auto.ApprovedAmount
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
