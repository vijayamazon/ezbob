namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class DefaultOutstandingLoans : ADefaultLoans {
		public DefaultOutstandingLoans(
			ASafeLog log,
			ExcelWorksheet sheet,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved
		) : base(
			"Default outstanding loans",
			log,
			sheet,
			total,
			manuallyApproved,
			autoApproved
		) {} // constructor

		protected override LoanCount.CountAmount GetCountAmount(ManualDatumItem item) {
			return item.LoanCount.DefaultOutstanding;
		} // GetCountAmount
	} // class DefaultOutstandingLoans
} // namespace
