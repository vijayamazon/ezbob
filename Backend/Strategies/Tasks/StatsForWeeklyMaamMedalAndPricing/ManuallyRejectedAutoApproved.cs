namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using OfficeOpenXml;

	internal class ManuallyRejectedAutoApproved : ARejectedCrossApproved {
		public ManuallyRejectedAutoApproved(
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
			return new[] { rejected, approved, };
		} // OrderApprovedAndRejected
	} // class ManuallyRejectedAutoApproved
} // namespace
