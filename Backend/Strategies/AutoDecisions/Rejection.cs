namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using System;
	using System.Data;
	using Ezbob.Logger;

	public class Rejection
	{
		private readonly AutoDecisionRequest request;
		private readonly int autoRejectionExceptionAnualTurnover;
		private readonly int rejectDefaultsCreditScore;
		private readonly int rejectMinimalSeniority;
		private readonly int lowCreditScore;
		private readonly int rejectDefaultsAccountsNum;
		private readonly int autoRejectionExceptionCreditScore;
		private readonly bool hasAccountingAccounts;
		private readonly int errorMPsNum;
		private readonly int loanOfferApprovalNum;
		private readonly int numOfDefaultAccounts;
		private readonly AConnection Db;
		private readonly ASafeLog log;

		public Rejection(AutoDecisionRequest request, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.request = request;
			DataTable dt = Db.ExecuteReader("GetRejectionConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];

			autoRejectionExceptionAnualTurnover = int.Parse(results["AutoRejectionException_AnualTurnover"].ToString());
			rejectDefaultsCreditScore = int.Parse(results["Reject_Defaults_CreditScore"].ToString());
			rejectMinimalSeniority = int.Parse(results["Reject_Minimal_Seniority"].ToString());
			lowCreditScore = int.Parse(results["LowCreditScore"].ToString());
			rejectDefaultsAccountsNum = int.Parse(results["Reject_Defaults_AccountsNum"].ToString());
			autoRejectionExceptionCreditScore = int.Parse(results["AutoRejectionException_CreditScore"].ToString());
			int rejectDefaultsMonths = int.Parse(results["Reject_Defaults_MonthsNum"].ToString());
			int rejectDefaultsAmount = int.Parse(results["Reject_Defaults_Amount"].ToString());

			dt = Db.ExecuteReader(
				"GetCustomerRejectionData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", request.CustomerId),
				new QueryParameter("Reject_Defaults_Months", rejectDefaultsMonths),
				new QueryParameter("Reject_Defaults_Amount", rejectDefaultsAmount)
			);

			results = dt.Rows[0];

			hasAccountingAccounts = Convert.ToBoolean(results["HasAccountingAccounts"]);
			errorMPsNum = int.Parse(results["ErrorMPsNum"].ToString());
			loanOfferApprovalNum = int.Parse(results["ApprovalNum"].ToString());
			numOfDefaultAccounts = int.Parse(results["NumOfDefaultAccounts"].ToString());
		}

		private bool IsException()
		{
			if (loanOfferApprovalNum > 0 || request.TotalSumOfOrders1YTotal > autoRejectionExceptionAnualTurnover ||
				request.InitialExperianConsumerScore > autoRejectionExceptionCreditScore || errorMPsNum > 0 ||
				(decimal)request.InitialExperianConsumerScore == 0 || hasAccountingAccounts)
			{
				return true;
			}

			return false;
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (IsException())
				return false;

			DataTable dt = Db.ExecuteReader(
				"GetPayPalAggregations",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", request.CustomerId)
			);

			DataRow results = dt.Rows[0];

			response.PayPalNumberOfStores = int.Parse(results["PayPal_NumberOfStores"].ToString());
			response.PayPalTotalSumOfOrders3M = decimal.Parse(results["PayPal_TotalSumOfOrders3M"].ToString());
			response.PayPalTotalSumOfOrders1Y = decimal.Parse(results["PayPal_TotalSumOfOrders1Y"].ToString());

			if (request.InitialExperianConsumerScore < rejectDefaultsCreditScore &&
				numOfDefaultAccounts >= rejectDefaultsAccountsNum)
			{
				response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" + request.InitialExperianConsumerScore +
								   " < " + rejectDefaultsCreditScore + " AND " + numOfDefaultAccounts + " >= " +
								   rejectDefaultsAccountsNum;
			}
			else if (request.InitialExperianConsumerScore < lowCreditScore)
			{
				response.AutoRejectReason = "AutoReject: Low score. Condition not met:" + request.InitialExperianConsumerScore + " < " +
								   lowCreditScore;
			}
			else if (
				(response.PayPalNumberOfStores == 0 ||
				 response.PayPalTotalSumOfOrders3M < request.LowTotalThreeMonthTurnover || response.PayPalTotalSumOfOrders1Y < request.LowTotalAnnualTurnover)
				 &&
				(request.TotalSumOfOrders3MTotal < request.LowTotalThreeMonthTurnover || request.TotalSumOfOrders1YTotal < request.LowTotalAnnualTurnover)
			   )
			{
				response.AutoRejectReason = "AutoReject: Totals. Condition not met: (" + response.PayPalNumberOfStores + " < 0 OR" +
								response.PayPalTotalSumOfOrders3M + " < " +
								   request.LowTotalThreeMonthTurnover + " OR " + response.PayPalTotalSumOfOrders1Y + " < " +
								   request.LowTotalAnnualTurnover + ") AND (" + request.TotalSumOfOrders3MTotal + " < " +
								   request.LowTotalThreeMonthTurnover + " OR " + request.TotalSumOfOrders1YTotal + " < " +
								   request.LowTotalAnnualTurnover + ")";
			}
			else if (request.MarketplaceSeniorityDays < rejectMinimalSeniority && errorMPsNum == 0)
			{
				response.AutoRejectReason = "AutoReject: Seniority. Condition not met: (" + request.MarketplaceSeniorityDays + " < " +
								   rejectMinimalSeniority + ")";
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
