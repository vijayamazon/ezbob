namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyApprovedAutoRejected : ARejectedCrossApproved {
		public ManuallyApprovedAutoRejected(
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			AStatItem rejected,
			AStatItem approved
		) : base(
			log.Safe(),
			sheet,
			title,
			total,
			rejected,
			approved
		) {} // constructor

		protected override TitledValue[] OrderApprovedAndRejected(TitledValue rejected, TitledValue approved) {
			return new[] { approved, rejected, };
		} // OrderApprovedAndRejected
	} // class ManuallyApprovedAutoRejected
} // namespace
