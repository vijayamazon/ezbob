namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class AutoProcessed : AStatItem {
		public AutoProcessed(ASafeLog log, ExcelWorksheet sheet, Total total) : base(
			log,
			sheet,
			"Auto processed",
			total
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			Added.If(d.Auto(cashRequestIndex).HasDecided);
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
					new TitledValue("processed / total %", Count, Superior[0].Count),
				},
			};
		} // PrepareMultipleCountRowValues
	} // class AutoProcessed
} // namespace
