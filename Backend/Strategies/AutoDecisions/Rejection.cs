﻿namespace EzBob.Backend.Strategies.AutoDecisions
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

		
		public bool HasAccountingAccounts { get; private set; } 
		public int ErrorMPsNum { get; private set; } 
		public int LoanOffer_ApprovalNum { get; private set; } 
		public int NumOfDefaultAccounts { get; private set; } 

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
		}

		private bool IsException(AutoDecisionResponse response)
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

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (IsException(response))
			{
				return true;
			}

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
