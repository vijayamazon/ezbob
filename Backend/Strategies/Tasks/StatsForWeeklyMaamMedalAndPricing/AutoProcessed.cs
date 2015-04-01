﻿namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class AutoProcessed : AStatItem {
		public AutoProcessed(ExcelWorksheet sheet, Total total) : base(
			sheet,
			"Auto processed",
			total
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			Added.If(d.Auto(cashRequestIndex).HasDecided);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("processed / total %", Count, Superior[0].Count),
			};
		} // PrepareCountRowValues
	} // class AutoProcessed
} // namespace
