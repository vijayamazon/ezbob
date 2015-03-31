namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using OfficeOpenXml;

	internal class ManuallyApprovedAutoRejected : ARejectedCrossApproved {
		public ManuallyApprovedAutoRejected(
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

		protected override TitledValue[] OrderApprovedAndRejected(TitledValue rejected, TitledValue approved) {
			return new[] { approved, rejected, };
		} // OrderApprovedAndRejected
	} // class ManuallyApprovedAutoRejected
} // namespace
