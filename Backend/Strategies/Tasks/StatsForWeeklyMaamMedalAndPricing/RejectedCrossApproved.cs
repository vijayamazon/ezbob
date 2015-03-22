namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using OfficeOpenXml;

	internal class RejectedCrossApproved : AStatItem {
		public RejectedCrossApproved(
			ExcelWorksheet sheet,
			string title,
			Total total,
			AStatItem rejected,
			AStatItem approved
		) : base(
			sheet,
			title,
			total,
			rejected,
			approved
		) {} // constructor

		public override void Add(Datum d) {
			Added.If(Rejected.LastWasAdded && Approved.LastWasAdded, Approved.LastAmount);
		} // Add

		protected override TitledValue[] PrepareCountRowValues() {
			return new[] {
				new TitledValue("count", Count),
				new TitledValue("count / total %", Count, Total.Count),
				new TitledValue("count / rejected %", Count, Rejected.Count),
				new TitledValue("count / approved %", Count, Approved.Count),
			};
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return new[] {
				new TitledValue("amount", Amount),
				new TitledValue("amount / approved %", Amount, Approved.Amount),
			};
		} // PrepareAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem Rejected { get { return Superior[1]; } }
		private AStatItem Approved { get { return Superior[2]; } }
	} // class RejectedCrossApproved
} // namespace
