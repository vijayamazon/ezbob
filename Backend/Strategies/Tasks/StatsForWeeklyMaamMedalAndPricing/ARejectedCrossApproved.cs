namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal abstract class ARejectedCrossApproved : AStatItem {
		public abstract int DrawSummary(int row);

		protected ARejectedCrossApproved(
			bool takeMin,
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			AStatItem superOne,
			AStatItem superTwo
		) : base(
			log,
			sheet,
			title,
			total,
			superOne,
			superTwo
		) {
			LoanCount = new LoanCount(takeMin, Log);
		} // constructor

		protected override TitledValue[] PrepareCountRowValues() {
			return null;
		} // PrepareCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return null;
		} // PrepareAmountRowValues

		protected LoanCount LoanCount { get; set; }

		protected AStatItem Total { get { return Superior[0]; } }
	} // class RejectedCrossApproved
} // namespace
