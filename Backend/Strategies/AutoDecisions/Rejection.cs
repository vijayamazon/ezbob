namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System.Data;
	using DbConnection;

	public class Rejection
	{
		private readonly AutoDecisionRequest request;
		private readonly int AutoRejectionException_AnualTurnover;
		private readonly int Reject_Defaults_CreditScore;
		private readonly int Reject_Minimal_Seniority;
		private readonly int LowCreditScore;
		private readonly int Reject_Defaults_AccountsNum;
		private readonly int AutoRejectionException_CreditScore;
		private readonly bool HasAccountingAccounts;
		private readonly int ErrorMPsNum;
		private readonly int LoanOffer_ApprovalNum;
		private readonly int NumOfDefaultAccounts;

		public Rejection(AutoDecisionRequest request)
		{
			this.request = request;
			DataTable dt = DbConnection.ExecuteSpReader("GetRejectionConfigs");
			DataRow results = dt.Rows[0];

			AutoRejectionException_AnualTurnover = int.Parse(results["AutoRejectionException_AnualTurnover"].ToString());
			Reject_Defaults_CreditScore = int.Parse(results["Reject_Defaults_CreditScore"].ToString());
			Reject_Minimal_Seniority = int.Parse(results["Reject_Minimal_Seniority"].ToString());
			LowCreditScore = int.Parse(results["LowCreditScore"].ToString());
			Reject_Defaults_AccountsNum = int.Parse(results["Reject_Defaults_AccountsNum"].ToString());
			AutoRejectionException_CreditScore = int.Parse(results["AutoRejectionException_CreditScore"].ToString());
			int Reject_Defaults_Months = int.Parse(results["Reject_Defaults_MonthsNum"].ToString());
			int Reject_Defaults_Amount = int.Parse(results["Reject_Defaults_Amount"].ToString());

			dt = DbConnection.ExecuteSpReader("GetCustomerRejectionData", DbConnection.CreateParam("CustomerId", request.CustomerId)
				, DbConnection.CreateParam("Reject_Defaults_Months", Reject_Defaults_Months)
				, DbConnection.CreateParam("Reject_Defaults_Amount", Reject_Defaults_Amount));
			results = dt.Rows[0];

			HasAccountingAccounts = bool.Parse(results["HasAccountingAccounts"].ToString());
			ErrorMPsNum = int.Parse(results["ErrorMPsNum"].ToString());
			LoanOffer_ApprovalNum = int.Parse(results["ApprovalNum"].ToString());
			NumOfDefaultAccounts = int.Parse(results["NumOfDefaultAccounts"].ToString());
		}

		private bool IsException()
		{
			if (LoanOffer_ApprovalNum > 0 || request.TotalSumOfOrders1YTotal > AutoRejectionException_AnualTurnover ||
				request.Inintial_ExperianConsumerScore > AutoRejectionException_CreditScore || ErrorMPsNum > 0 ||
				(decimal)request.Inintial_ExperianConsumerScore == 0 || HasAccountingAccounts)
			{
				return true;
			}

			return false;
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (IsException())
			{
				return false;
			}

			DataTable dt = DbConnection.ExecuteSpReader("GetPayPalAggregations",
														DbConnection.CreateParam("CustomerId", request.CustomerId));
			DataRow results = dt.Rows[0];

			response.PayPal_NumberOfStores = int.Parse(results["PayPal_NumberOfStores"].ToString());
			response.PayPal_TotalSumOfOrders3M = decimal.Parse(results["PayPal_TotalSumOfOrders3M"].ToString());
			response.PayPal_TotalSumOfOrders1Y = decimal.Parse(results["PayPal_TotalSumOfOrders1Y"].ToString());

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
				(response.PayPal_NumberOfStores == 0 ||
				 response.PayPal_TotalSumOfOrders3M < request.LowTotalThreeMonthTurnover || response.PayPal_TotalSumOfOrders1Y < request.LowTotalAnnualTurnover)
				 &&
				(request.TotalSumOfOrders3MTotal < request.LowTotalThreeMonthTurnover || request.TotalSumOfOrders1YTotal < request.LowTotalAnnualTurnover)
			   )
			{
				response.AutoRejectReason = "AutoReject: Totals. Condition not met: (" + response.PayPal_NumberOfStores + " < 0 OR" +
								response.PayPal_TotalSumOfOrders3M + " < " +
								   request.LowTotalThreeMonthTurnover + " OR " + response.PayPal_TotalSumOfOrders1Y + " < " +
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
				return false;
			}

			response.CreditResult = request.EnableAutomaticRejection ? "WaitingForDecision" : "Rejected";
			response.UserStatus = "Rejected";
			response.SystemDecision = "Reject";
			response.ModelLoanOffer = 0;

			return true;
		}
	}
}
