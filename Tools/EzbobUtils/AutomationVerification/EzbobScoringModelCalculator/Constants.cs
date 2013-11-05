namespace EzbobScoringModelCalculator
{
	public static class Constants
	{
		#region Weight Constants
		public static double ExperianFirstRepaymentPassedWeight = 0.32;
		public static double ExperianFirstRepaymentNotPassedWeight = 0.40;

		public static double MarketplaceSeniorityRepaymentPassedWeight = 0.10;
		public static double MarketplaceSeniorityRepaymentNotPassedWeight = 0.14;

		public static double MaritalStatusWeight = 0.08;

		public static double PositiveFeedbackCountWeight = 0.18;

		public static double GenderWeight = 0.06; //??? 

		public static double AnnualTurnoverFirstRepaymentPassedWeight = 0.07;
		public static double AnnualTurnoverFirstRepaymentNotPassedWeight = 0.10;

		public static double NumberOfStoresWeight = 0.04; //ebay amazon paypal

		public static double EzbobSeniorityFirstRepaymentPassedWeight = 0.03;
		public static double EzbobSeniorityFirstRepaymentNotPassedWeight = 0;

		public static double EzbobNumberOfLoansFirstRepaymentPassedWeight = 0.05;
		public static double EzbobNumberOfLoansFirstRepaymentNotPassedWeight = 0;

		public static double EzbobPreviousLatePaymentsFirstRepaymentPassedWeight = 0.04;
		public static double EzbobPreviousLatePaymentsFirstRepaymentNotPassedWeight = 0;

		public static double EzbobPreviousEarlyPaymentsFirstRepaymentPassedWeight = 0.03;
		public static double EzbobPreviousEarlyPaymentsFirstRepaymentNotPassedWeight = 0;
		
		#endregion Weight 

	}
}
