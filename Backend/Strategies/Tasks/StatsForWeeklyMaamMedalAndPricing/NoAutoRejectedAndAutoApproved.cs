namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class NotAutoRejectedAndAutoApproved : AStatItem {
		public NotAutoRejectedAndAutoApproved(
			ExcelWorksheet sheet,
			Total total,
			AutoRejected autoRejected,
			AutoApproved autoApproved
		) : base(
			sheet,
			"Not auto rejected & auto approved",
			total,
			autoRejected,
			autoApproved
		) {} // constructor

		public override void Add(Datum d) {
			Added.If(
				!AutoRejected.LastWasAdded && AutoApproved.LastWasAdded,
				AutoApproved.LastAmount
			);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("not auto rejected count", Total.Count - AutoRejected.Count),
				new TitledValue("out of them auto approved count", Count),
				new TitledValue("auto approved / not auto rejected %", Count, Total.Count - AutoRejected.Count),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("amount", Amount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem AutoRejected { get { return Superior[1]; } }
		private AStatItem AutoApproved { get { return Superior[2]; } }
	} // class NotAutoRejectedAndAutoApproved
} // namespace
