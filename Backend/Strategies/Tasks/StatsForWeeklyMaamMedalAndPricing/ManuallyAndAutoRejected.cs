namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyAndAutoRejected : AStatItem {
		public ManuallyAndAutoRejected(
			ASafeLog log,
			ExcelWorksheet sheet,
			Total total,
			ManuallyRejected manuallyRejected,
			AutoRejected autoRejected
		) : base(
			log.Safe(),
			sheet,
			"Manually and auto rejected",
			total,
			manuallyRejected,
			autoRejected
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			Added.If(ManuallyRejected.LastWasAdded && AutoRejected.LastWasAdded);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("rejected / total %", Count, Total.Count),
				new TitledValue("rejected / manually rejected %", Count, ManuallyRejected.Count),
				new TitledValue("rejected / auto rejected %", Count, AutoRejected.Count),
			};
		} // PrepareCountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem ManuallyRejected { get { return Superior[1]; } }
		private AStatItem AutoRejected { get { return Superior[2]; } }
	} // class ManuallyAndAutoRejected
} // namespace
