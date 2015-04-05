﻿namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	// using DbConstants;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class AutoRerejected : AStatItem {
		public AutoRerejected(
			ASafeLog log,
			ExcelWorksheet sheet,
			Total total,
			AutoProcessed autoProcessed,
			AutoRejected autoRejected
		) : base(
			log,
			sheet,
			"Auto re-rejected",
			total,
			autoProcessed,
			autoRejected
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			// TODO Added.If(d.AutomationDecision == DecisionActions.ReReject);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("re-rejected / total %", Count, Total.Count),
				new TitledValue("re-rejected / processed %", Count, AutoProcessed.Count),
				new TitledValue("re-rejected / rejected %", Count, AutoRejected.Count),
			};
		} // PrepareCountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem AutoProcessed { get { return Superior[1]; } }
		private AStatItem AutoRejected { get { return Superior[2]; } }
	} // class AutoRerejected
} // namespace
