namespace EzBob.Backend.Strategies.AutoDecisions
{
	using EzBob.Models;
	using Ezbob.Database;
	using System;
	using System.Data;
	using Ezbob.Logger;

	public class ReApproval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly int autoReApproveMaxNumOfOutstandingLoans;
		private readonly AutoDecisionRequest request;
		private readonly AConnection Db;
		private readonly ASafeLog log;

		public ReApproval(AutoDecisionRequest request, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.request = request;
			DataTable dt = Db.ExecuteReader("GetReApprovalConfigs", CommandSpecies.StoredProcedure); 
			var sr = new SafeReader(dt.Rows[0]);
			autoReApproveMaxNumOfOutstandingLoans = sr["AutoReApproveMaxNumOfOutstandingLoans"];
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

			var sr = new SafeReader(dt.Rows[0]);
			bool loanOfferEmailSendingBanned = sr["EmailSendingBanned"];
			DateTime loanOfferOfferStart = sr["OfferStart"];
			DateTime loanOfferOfferValidUntil = sr["OfferValidUntil"];
			int loanOfferSystemCalculatedSum = sr["SystemCalculatedSum"];
			int loanOfferSumOfChargesOld = sr["SumOfChargesOld"];
			int loanOfferNumOfMPsAddedOld = sr["NumOfMPsAddedOld"];
			decimal loanOfferPrincipalPaidAmountOld = sr["PrincipalPaidAmountOld"];

			if ((request.LoanOfferReApprovalFullAmount > 0 || request.LoanOfferReApprovalRemainingAmount > 0) ||
				((request.LoanOfferReApprovalFullAmountOld > 0 || request.LoanOfferReApprovalRemainingAmountOld > 0) &&
			     loanOfferPrincipalPaidAmountOld == 0 && loanOfferSumOfChargesOld == 0 &&
			     loanOfferNumOfMPsAddedOld == 0))
			{
				dt = Db.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure); 
				sr = new SafeReader(dt.Rows[0]);
				decimal availableFunds = sr["AvailableFunds"];
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
