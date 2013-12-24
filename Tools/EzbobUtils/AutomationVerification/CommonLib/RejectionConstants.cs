namespace CommonLib
{
	public class RejectionConstants
	{
		public int MinCreditScore { get; set; }
		public int MinAnnualTurnover { get; set; }
		public int MinThreeMonthTurnover { get; set; }
		public int DefaultScoreBelow { get; set; }
		public int DefaultMinAmount { get; set; }
		public int DefaultMinMonths { get; set; }
		public int MinMarketPlaceSeniorityDays { get; set; }
		public int NoRejectIfTotalAnnualTurnoverAbove { get; set; }
		public int NoRejectIfCreditScoreAbove { get; set; }
	}
}
