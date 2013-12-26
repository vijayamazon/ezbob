namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using System;
	using System.Data;
	using Models;

	public class ReApproval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly int autoReApproveMaxNumOfOutstandingLoans;
		private readonly AutoDecisionRequest request;
		private AConnection Db { get; set; }

		public ReApproval(AutoDecisionRequest request, AConnection oDb) {
			Db = oDb;
			this.request = request;
			DataTable dt = Db.ExecuteReader("GetReApprovalConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			autoReApproveMaxNumOfOutstandingLoans = int.Parse(results["AutoReApproveMaxNumOfOutstandingLoans"].ToString());
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			DataTable dt = Db.ExecuteReader(
				"GetLastOfferDataForReApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", request.CustomerId)
			);

			if (dt.Rows.Count == 0) // Cant reapprove without previous approvals
			{
				return false;
			}

			DataRow results = dt.Rows[0];
			bool loanOfferEmailSendingBanned = Convert.ToBoolean(results["EmailSendingBanned"]);
			DateTime loanOfferOfferStart = DateTime.Parse(results["OfferStart"].ToString());
			DateTime loanOfferOfferValidUntil = DateTime.Parse(results["OfferValidUntil"].ToString());
			int loanOfferSystemCalculatedSum = int.Parse(results["SystemCalculatedSum"].ToString());
			int loanOfferSumOfChargesOld = int.Parse(results["SumOfChargesOld"].ToString());
			int loanOfferNumOfMPsAddedOld = int.Parse(results["NumOfMPsAddedOld"].ToString());
			int loanOfferPrincipalPaidAmountOld = int.Parse(results["PrincipalPaidAmountOld"].ToString());

			if ((request.LoanOfferReApprovalFullAmount > 0 || request.LoanOfferReApprovalRemainingAmount > 0) ||
				((request.LoanOfferReApprovalFullAmountOld > 0 || request.LoanOfferReApprovalRemainingAmountOld > 0) &&
			     loanOfferPrincipalPaidAmountOld == 0 && loanOfferSumOfChargesOld == 0 &&
			     loanOfferNumOfMPsAddedOld == 0))
			{
				dt = Db.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure);
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
					response.LoanOfferUnderwriterComment = "Auto Re-Approval";
					response.LoanOfferOfferValidDays =
						(loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays;
					response.AppApplyForLoan = null;
					response.AppValidFor = DateTime.UtcNow.AddDays(response.LoanOfferOfferValidDays);
					response.LoanOfferEmailSendingBannedNew = loanOfferEmailSendingBanned;
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
