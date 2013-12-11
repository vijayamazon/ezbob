namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Data;
	using DbConnection;
	using Models;

	public class ReApproval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly int autoReApproveMaxNumOfOutstandingLoans;
		private readonly AutoDecisionRequest request;

		public ReApproval(AutoDecisionRequest request)
		{
			this.request = request;
			DataTable dt = DbConnection.ExecuteSpReader("GetReApprovalConfigs");
			DataRow results = dt.Rows[0];
			autoReApproveMaxNumOfOutstandingLoans = int.Parse(results["AutoReApproveMaxNumOfOutstandingLoans"].ToString());
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetLastOfferDataForReApproval", DbConnection.CreateParam("CustomerId", request.CustomerId));
			DataRow results = dt.Rows[0];
			bool loanOfferEmailSendingBanned = bool.Parse(results["EmailSendingBanned"].ToString());
			DateTime loanOfferOfferStart = DateTime.Parse(results["OfferStart"].ToString());
			DateTime loanOfferOfferValidUntil = DateTime.Parse(results["OfferValidUntil"].ToString());
			int loanOfferSystemCalculatedSum = int.Parse(results["SystemCalculatedSum"].ToString());
			int loanOfferSumOfChargesOld = int.Parse(results["SumOfChargesOld"].ToString());
			int loanOfferNumOfMPsAddedOld = int.Parse(results["NumOfMPsAddedOld"].ToString());
			int loanOfferPrincipalPaidAmountOld = int.Parse(results["PrincipalPaidAmountOld"].ToString());

			if ((request.LoanOffer_ReApprovalFullAmount > 0 || request.LoanOffer_ReApprovalRemainingAmount > 0) ||
				((request.LoanOffer_ReApprovalFullAmountOld > 0 || request.LoanOffer_ReApprovalRemainingAmountOld > 0) &&
			     loanOfferPrincipalPaidAmountOld == 0 && loanOfferSumOfChargesOld == 0 &&
			     loanOfferNumOfMPsAddedOld == 0))
			{
				dt = DbConnection.ExecuteSpReader("GetAvailableFunds");
				decimal availableFunds = decimal.Parse(dt.Rows[0]["AvailableFunds"].ToString());
				if (availableFunds > loanOfferSystemCalculatedSum)
				{
					int numOfOutstandingLoans = strategyHelper.GetOutstandingLoansNum(request.CustomerId);
					if (numOfOutstandingLoans > autoReApproveMaxNumOfOutstandingLoans)
					{
						response.CreditResult = "WaitingForDecision";
						response.UserStatus = "Manual";
						response.SystemDecision = "Manual";
						return true;
					}

					response.CreditResult = request.EnableAutomaticReApproval ? "Approved" : "WaitingForDecision";
					response.UserStatus = "Approved";
					response.SystemDecision = "Approve";
					response.LoanOffer_UnderwriterComment = "Auto Re-Approval";
					response.LoanOffer_OfferValidDays =
						(loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays;
					response.App_ApplyForLoan = null;
					response.App_ValidFor = DateTime.UtcNow.AddDays(request.LoanOffer_OfferValidDays);
					response.LoanOffer_EmailSendingBanned_new = loanOfferEmailSendingBanned;
					return true;
				}

				response.CreditResult = "WaitingForDecision";
				response.UserStatus = "Manual";
				response.SystemDecision = "Manual";
				return true;
			}

			return false;
		}
	}
}
