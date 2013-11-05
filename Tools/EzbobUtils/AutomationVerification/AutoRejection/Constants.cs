namespace AutoRejection
{
	public static class Constants
	{
		public static int MinCreditScore = 550;
		
		public static int MinAnnualTurnover = 10000;
		
		public static int MinThreeMonthTurnover = 2000;
		
		public static int DefaultScoreBelow = 800;
		public static int DefaultMinAmount = 300;
		public static int DefaultMinMonths = 12;

		public static int MinMarketPlaceSeniorityDays = 300;
		public static bool AutoRejectIfErrorInAtLeastOneMarketPlace = false;

		public static bool AutoRejectIfWasApprovedOnce = false;

		public static int NoRejectIfTotalAnnualTurnoverAbove = 250000;

		public static int NoRejectIfCreditScoreAbove = 900;
	}
}
