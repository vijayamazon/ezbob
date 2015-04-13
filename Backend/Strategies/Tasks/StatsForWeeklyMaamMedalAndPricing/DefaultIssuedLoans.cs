namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class DefaultIssuedLoans : ADefaultLoans {
		public DefaultIssuedLoans(
			ASafeLog log,
			ExcelWorksheet sheet,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved
		) : base(
			"Default issued loans",
			log,
			sheet,
			total,
			manuallyApproved,
			autoApproved
		) {} // constructor

		protected override LoanCount.CountAmount GetCountAmount(ManualDatumItem item) {
			return item.LoanCount.DefaultIssued;
		} // GetCountAmount
	} // class DefaultIssuedLoans
} // namespace
