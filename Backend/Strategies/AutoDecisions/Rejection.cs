namespace EzBob.Backend.Strategies.AutoDecisions
{
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

		private bool IsException(AutoDecisionRequest request, AutoDecisionResponse response)
		{
			if (LoanOffer_ApprovalNum > 0 || request.TotalSumOfOrders1YTotal > AutoRejectionException_AnualTurnover ||
				request.Inintial_ExperianConsumerScore > AutoRejectionException_CreditScore || ErrorMPsNum > 0 ||
				(decimal)request.Inintial_ExperianConsumerScore == 0 || HasAccountingAccounts)
			{
				response.CreditResult = "WaitingForDecision";
				response.UserStatus = "Manual";
				response.SystemDecision = "Manual";
				return true;
			}

			return false;
		}

		public bool MakeDecision(AutoDecisionRequest request, AutoDecisionResponse response)
		{
			if (IsException(request, response))
			{
				return true;
			}

			// Rejection
			if (request.Inintial_ExperianConsumerScore < Reject_Defaults_CreditScore &&
				NumOfDefaultAccounts >= Reject_Defaults_AccountsNum)
			{
				response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" + request.Inintial_ExperianConsumerScore +
								   " < " + Reject_Defaults_CreditScore + " AND " + NumOfDefaultAccounts + " >= " +
								   Reject_Defaults_AccountsNum;
			}
			else if (request.Inintial_ExperianConsumerScore < LowCreditScore)
			{
				response.AutoRejectReason = "AutoReject: Low score. Condition not met:" + request.Inintial_ExperianConsumerScore + " < " +
								   LowCreditScore;
			}
			else if (
				(request.PayPal_NumberOfStores == 0 ||
				 request.PayPal_TotalSumOfOrders3M < request.LowTotalThreeMonthTurnover || request.PayPal_TotalSumOfOrders1Y < request.LowTotalAnnualTurnover)
				 &&
				(request.TotalSumOfOrders3MTotal < request.LowTotalThreeMonthTurnover || request.TotalSumOfOrders1YTotal < request.LowTotalAnnualTurnover)
			   )
			{
				response.AutoRejectReason = "AutoReject: Totals. Condition not met: (" + request.PayPal_NumberOfStores + " < 0 OR" +
								request.PayPal_TotalSumOfOrders3M + " < " +
								   request.LowTotalThreeMonthTurnover + " OR " + request.PayPal_TotalSumOfOrders1Y + " < " +
								   request.LowTotalAnnualTurnover + ") AND (" + request.TotalSumOfOrders3MTotal + " < " +
								   request.LowTotalThreeMonthTurnover + " OR " + request.TotalSumOfOrders1YTotal + " < " +
								   request.LowTotalAnnualTurnover + ")";
			}
			else if (request.MarketplaceSeniorityDays < Reject_Minimal_Seniority && ErrorMPsNum == 0)
			{
				response.AutoRejectReason = "AutoReject: Seniority. Condition not met: (" + request.MarketplaceSeniorityDays + " < " +
								   Reject_Minimal_Seniority + ")";
			}
			else
			{
				response.CreditResult = "WaitingForDecision";
				response.UserStatus = "Manual";
				response.SystemDecision = "Manual";
				return true;
			}

			response.CreditResult = request.EnableAutomaticRejection ? "WaitingForDecision" : "Rejected";
			response.UserStatus = "Rejected";
			response.SystemDecision = "Reject";
			response.ModelLoanOffer = 0;

			return true;
		}
	}
}
