namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyApproved : AStatItem {
		public ManuallyApproved(ASafeLog log, ExcelWorksheet sheet, Total total) : base(
			log.Safe(),
			sheet,
			"Manually approved",
			total
		) {
			LoanCount = new LoanCount(true, Log);
		} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(d.Manual(cashRequestIndex).IsApproved, d.Manual(cashRequestIndex).ApprovedAmount))
				LoanCount += d.Manual(cashRequestIndex).ActualLoanCount;
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
					new TitledValue("approved / total %", Count, Total.Count),
				},
				new[] {
					new TitledValue("loan count", LoanCount.Total.Count),
				},
			};
		} // PrepareMultipleCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return null;
		} // PrepareAmountRowValues

		protected override List<TitledValue[]> PrepareMultipleAmountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("approved amount", Amount),
					new TitledValue("issued amount", LoanCount.Total.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }

		public LoanCount LoanCount { get; private set; }
	} // class ManuallyApproved
} // namespace
