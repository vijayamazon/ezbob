namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Data;
	using Backend.Strategies;
	using DbConnection;
	using Models;

	public class ReApproval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();

		public ReApproval()
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetReApprovalConfigs");
			DataRow results = dt.Rows[0];
			autoReApproveMaxNumOfOutstandingLoans = int.Parse(results["AutoReApproveMaxNumOfOutstandingLoans"].ToString());
		}

		private readonly int autoReApproveMaxNumOfOutstandingLoans;
		private decimal availableFunds;

		public bool MakeDecision(MainStrategy mainStrategy)
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetLastOfferDataForReApproval", DbConnection.CreateParam("CustomerId", mainStrategy.CustomerId));
			DataRow results = dt.Rows[0];
			bool loanOfferEmailSendingBanned = bool.Parse(results["EmailSendingBanned"].ToString());
			DateTime loanOfferOfferStart = DateTime.Parse(results["OfferStart"].ToString());
			DateTime loanOfferOfferValidUntil = DateTime.Parse(results["OfferValidUntil"].ToString());
			int loanOfferSystemCalculatedSum = int.Parse(results["SystemCalculatedSum"].ToString());
			int loanOfferSumOfChargesOld = int.Parse(results["SumOfChargesOld"].ToString());
			int loanOfferNumOfMPsAddedOld = int.Parse(results["NumOfMPsAddedOld"].ToString());
			int loanOfferPrincipalPaidAmountOld = int.Parse(results["PrincipalPaidAmountOld"].ToString());

			if ((mainStrategy.LoanOffer_ReApprovalFullAmount > 0 || mainStrategy.LoanOffer_ReApprovalRemainingAmount > 0) ||
			    ((mainStrategy.LoanOffer_ReApprovalFullAmountOld > 0 || mainStrategy.LoanOffer_ReApprovalRemainingAmountOld > 0) &&
			     loanOfferPrincipalPaidAmountOld == 0 && loanOfferSumOfChargesOld == 0 &&
			     loanOfferNumOfMPsAddedOld == 0))
			{
				dt = DbConnection.ExecuteSpReader("GetAvailableFunds");
				availableFunds = decimal.Parse(dt.Rows[0]["AvailableFunds"].ToString());
				if (availableFunds > loanOfferSystemCalculatedSum)
				{
					mainStrategy.NumOfOutstandingLoans = strategyHelper.GetOutstandingLoansNum(mainStrategy.CustomerId);
					if (mainStrategy.NumOfOutstandingLoans > autoReApproveMaxNumOfOutstandingLoans)
					{
						mainStrategy.CreditResult = "WaitingForDecision";
						mainStrategy.UserStatus = "Manual";
						mainStrategy.SystemDecision = "Manual";
						return true;
					}

					mainStrategy.CreditResult = mainStrategy.EnableAutomaticReApproval ? "Approved" : "WaitingForDecision";
					mainStrategy.UserStatus = "Approved";
					mainStrategy.SystemDecision = "Approve";
					mainStrategy.LoanOffer_UnderwriterComment = "Auto Re-Approval";
					mainStrategy.LoanOffer_OfferValidDays =
						(loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays;
					mainStrategy.App_ApplyForLoan = null;
					mainStrategy.App_ValidFor = DateTime.UtcNow.AddDays(mainStrategy.LoanOffer_OfferValidDays);
					mainStrategy.LoanOffer_EmailSendingBanned_new = loanOfferEmailSendingBanned;
					return true;
				}


				mainStrategy.CreditResult = "WaitingForDecision";
				mainStrategy.UserStatus = "Manual";
				mainStrategy.SystemDecision = "Manual";
				return true;
			}

			return false;
		}
	}
}
