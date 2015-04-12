namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyRejected : AStatItem {
		public ManuallyRejected(ASafeLog log, ExcelWorksheet sheet, Total total) : base(
			log.Safe(),
			sheet,
			"Manually rejected",
			total
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			Added.If(!d.Manual(cashRequestIndex).IsApproved);
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
				},
			};
		} // PrepareMultipleCountRowValues

		private AStatItem Total { get { return Superior[0]; } }
	} // class ManuallyRejected
} // namespace
