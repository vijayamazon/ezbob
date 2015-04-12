namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal abstract class ADefaultLoans : AStatItem {
		public override void Add(Datum d, int cashRequestIndex) {
			ManualDatumItem item = d.Manual(cashRequestIndex);

			LoanCount.CountAmount ca = GetCountAmount(item);

			if (Added.If(ca.Exist, ca.Amount, ca.Count))
				this.approvedAmount += item.ApprovedAmount;
		} // Add

		protected ADefaultLoans(
			string title,
			ASafeLog log,
			ExcelWorksheet sheet,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved
		) : base(
			log,
			sheet,
			title,
			total,
			manuallyApproved,
			autoApproved
		) {} // constructor

		protected abstract LoanCount.CountAmount GetCountAmount(ManualDatumItem item);

		protected override TitledValue[] PrepareCountRowValues() {
			return null;
		} // PrepareCountRowValues

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("count", Count),
				},
				new[] {
					new TitledValue("count / total %", Count, Total.Count),
					new TitledValue("count / manually approved count %", Count, ManuallyApproved.Count),
					new TitledValue("count / auto approved count %", Count, AutoApproved.Count),
				},
			};
		} // PrepareMultipleCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return null;
		} // PrepareAmountRowValues

		protected override List<TitledValue[]> PrepareMultipleAmountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("approved amount", this.approvedAmount),
				},
				new[] {
					new TitledValue("approved amount / manually approved amount %", this.approvedAmount, ManuallyApproved.Amount),
					new TitledValue("approved amount / auto approved amount %", this.approvedAmount, AutoApproved.Amount),
				},
				new[] {
					new TitledValue("loan amount", Amount),
				},
				new[] {
					new TitledValue("loan amount / manually approved amount %", Amount, ManuallyApproved.Amount),
					new TitledValue("loan amount / auto approved amount %", Amount, AutoApproved.Amount),
					new TitledValue("loan amount / issued amount %", Amount, (ManuallyApproved as ManuallyApproved).LoanCount.Total.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem ManuallyApproved { get { return Superior[1]; } }
		private AStatItem AutoApproved { get { return Superior[2]; } }

		private decimal approvedAmount;
	} // class ADefaultLoans
} // namespace
