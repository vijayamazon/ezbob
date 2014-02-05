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
		private readonly AConnection Db;
		private readonly ASafeLog log;
		private readonly bool enableAutomaticReApproval;
		private readonly int customerId;
		private readonly decimal loanOfferReApprovalFullAmount;
		private readonly decimal loanOfferReApprovalRemainingAmount;
		private readonly decimal loanOfferReApprovalFullAmountOld;
		private readonly decimal loanOfferReApprovalRemainingAmountOld;
		
		public ReApproval(int customerId, bool enableAutomaticReApproval, decimal loanOfferReApprovalFullAmount, decimal loanOfferReApprovalRemainingAmount, decimal loanOfferReApprovalFullAmountOld, decimal loanOfferReApprovalRemainingAmountOld, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.enableAutomaticReApproval = enableAutomaticReApproval;
			this.customerId = customerId;
			this.loanOfferReApprovalFullAmount = loanOfferReApprovalFullAmount;
			this.loanOfferReApprovalRemainingAmount = loanOfferReApprovalRemainingAmount;
			this.loanOfferReApprovalFullAmountOld = loanOfferReApprovalFullAmountOld;
			this.loanOfferReApprovalRemainingAmountOld = loanOfferReApprovalRemainingAmountOld;
			DataTable dt = Db.ExecuteReader("GetReApprovalConfigs", CommandSpecies.StoredProcedure); 
			var sr = new SafeReader(dt.Rows[0]);
			autoReApproveMaxNumOfOutstandingLoans = sr["AutoReApproveMaxNumOfOutstandingLoans"];
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			DataTable dt = Db.ExecuteReader(
				"GetLastOfferDataForReApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
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

			if ((loanOfferReApprovalFullAmount > 0 || loanOfferReApprovalRemainingAmount > 0) ||
				((loanOfferReApprovalFullAmountOld > 0 || loanOfferReApprovalRemainingAmountOld > 0) &&
			     loanOfferPrincipalPaidAmountOld == 0 && loanOfferSumOfChargesOld == 0 &&
			     loanOfferNumOfMPsAddedOld == 0))
			{
				dt = Db.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure); 
				sr = new SafeReader(dt.Rows[0]);
				decimal availableFunds = sr["AvailableFunds"];
				if (availableFunds > loanOfferSystemCalculatedSum)
				{
					int numOfOutstandingLoans = strategyHelper.GetOutstandingLoansNum(customerId);
					if (numOfOutstandingLoans > autoReApproveMaxNumOfOutstandingLoans)
					{
						response.CreditResult = "WaitingForDecision";
						response.UserStatus = "Manual";
						response.SystemDecision = "Manual";
						return true;
					}

					response.CreditResult = enableAutomaticReApproval ? "Approved" : "WaitingForDecision";
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
