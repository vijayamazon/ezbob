namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class AutoRejected : AStatItem {
		public AutoRejected(ASafeLog log, ExcelWorksheet sheet, Total total, AutoProcessed autoProcessed) : base(
			log,
			sheet,
			"Auto rejected",
			total,
			autoProcessed
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			Added.If(
				d.Auto(cashRequestIndex).HasDecided &&
				d.Auto(cashRequestIndex).AutomationDecision.In(DecisionActions.Reject, DecisionActions.ReReject)
			);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return null;
		} // PrepareCountRowValues

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("count", Count),
				},
				new[] {
					new TitledValue("rejected / total %", Count, Total.Count),
					new TitledValue("rejected / processed %", Count, AutoProcessed.Count),
				},
			};
		} // PrepareMultipleCountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem AutoProcessed { get { return Superior[1]; } }
	} // class AutoRejected
} // namespace
