namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Backend.Strategies;

	public class Rejection
	{
		public int LoanOffer_ApprovalNum { get; private set; }
		public int AutoRejectionException_AnualTurnover { get; private set; }
		public int Reject_Defaults_CreditScore { get; private set; }
		public int Reject_Minimal_Seniority { get; private set; }
		public int LowCreditScore { get; private set; }
		public int Reject_Defaults_AccountsNum { get; private set; }
		public int NumOfDefaultAccounts { get; private set; }
		public int AutoRejectionException_CreditScore { get; private set; }
		public bool HasAccountingAccounts { get; private set; }
		public int ErrorMPsNum { get; private set; }

		private bool IsException(MainStrategy mainStrategy)
		{
			if (LoanOffer_ApprovalNum > 0 || mainStrategy.TotalSumOfOrders1YTotal > AutoRejectionException_AnualTurnover ||
				mainStrategy.Inintial_ExperianConsumerScore > AutoRejectionException_CreditScore || ErrorMPsNum > 0 ||
				(decimal)mainStrategy.Inintial_ExperianConsumerScore == 0 || HasAccountingAccounts)
			{
				mainStrategy.CreditResult = "WaitingForDecision";
				mainStrategy.UserStatus = "Manual";
				mainStrategy.SystemDecision = "Manual";
				return true;
			}

			return false;
		}

		public bool MakeDecision(MainStrategy mainStrategy)
		{
			if (IsException(mainStrategy))
			{
				return true;
			}

			// Rejection
			if (mainStrategy.Inintial_ExperianConsumerScore < Reject_Defaults_CreditScore &&
				NumOfDefaultAccounts >= Reject_Defaults_AccountsNum)
			{
				mainStrategy.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" + mainStrategy.Inintial_ExperianConsumerScore +
								   " < " + Reject_Defaults_CreditScore + " AND " + NumOfDefaultAccounts + " >= " +
								   Reject_Defaults_AccountsNum;
			}
			else if (mainStrategy.Inintial_ExperianConsumerScore < LowCreditScore)
			{
				mainStrategy.AutoRejectReason = "AutoReject: Low score. Condition not met:" + mainStrategy.Inintial_ExperianConsumerScore + " < " +
								   LowCreditScore;
			}
			else if (
				(mainStrategy.PayPal_NumberOfStores == 0 ||
				 mainStrategy.PayPal_TotalSumOfOrders3M < mainStrategy.LowTotalThreeMonthTurnover || mainStrategy.PayPal_TotalSumOfOrders1Y < mainStrategy.LowTotalAnnualTurnover)
				 &&
				(mainStrategy.TotalSumOfOrders3MTotal < mainStrategy.LowTotalThreeMonthTurnover || mainStrategy.TotalSumOfOrders1YTotal < mainStrategy.LowTotalAnnualTurnover)
			   )
			{
				mainStrategy.AutoRejectReason = "AutoReject: Totals. Condition not met: (" + mainStrategy.PayPal_NumberOfStores + " < 0 OR" +
								mainStrategy.PayPal_TotalSumOfOrders3M + " < " +
								   mainStrategy.LowTotalThreeMonthTurnover + " OR " + mainStrategy.PayPal_TotalSumOfOrders1Y + " < " +
								   mainStrategy.LowTotalAnnualTurnover + ") AND (" + mainStrategy.TotalSumOfOrders3MTotal + " < " +
								   mainStrategy.LowTotalThreeMonthTurnover + " OR " + mainStrategy.TotalSumOfOrders1YTotal + " < " +
								   mainStrategy.LowTotalAnnualTurnover + ")";
			}
			else if (mainStrategy.MarketplaceSeniorityDays < Reject_Minimal_Seniority && ErrorMPsNum == 0)
			{
				mainStrategy.AutoRejectReason = "AutoReject: Seniority. Condition not met: (" + mainStrategy.MarketplaceSeniorityDays + " < " +
								   Reject_Minimal_Seniority + ")";
			}
			else
			{
				mainStrategy.CreditResult = "WaitingForDecision";
				mainStrategy.UserStatus = "Manual";
				mainStrategy.SystemDecision = "Manual";
				return true;
			}

			mainStrategy.CreditResult = mainStrategy.EnableAutomaticRejection ? "WaitingForDecision" : "Rejected";
			mainStrategy.UserStatus = "Rejected";
			mainStrategy.SystemDecision = "Reject";
			mainStrategy.ModelLoanOffer = 0;

			return true;
		}
	}
}
