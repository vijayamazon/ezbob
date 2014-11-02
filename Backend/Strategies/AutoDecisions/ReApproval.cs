namespace EzBob.Backend.Strategies.AutoDecisions
{
	using EzBob.Models;
	using Ezbob.Database;
	using System;
	using Ezbob.Logger;
	using Misc;

	public class ReApproval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly AConnection Db;
		private readonly ASafeLog log;
		private readonly int customerId;
		private readonly decimal loanOfferReApprovalFullAmount;
		private readonly decimal loanOfferReApprovalRemainingAmount;
		private readonly decimal loanOfferReApprovalFullAmountOld;
		private readonly decimal loanOfferReApprovalRemainingAmountOld;
		
		public ReApproval(int customerId, decimal loanOfferReApprovalFullAmount, decimal loanOfferReApprovalRemainingAmount, decimal loanOfferReApprovalFullAmountOld, decimal loanOfferReApprovalRemainingAmountOld, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.customerId = customerId;
			this.loanOfferReApprovalFullAmount = loanOfferReApprovalFullAmount;
			this.loanOfferReApprovalRemainingAmount = loanOfferReApprovalRemainingAmount;
			this.loanOfferReApprovalFullAmountOld = loanOfferReApprovalFullAmountOld;
			this.loanOfferReApprovalRemainingAmountOld = loanOfferReApprovalRemainingAmountOld;
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				var configSafeReader = Db.GetFirst("GetReApprovalConfigs", CommandSpecies.StoredProcedure);
				int autoReApproveMaxNumOfOutstandingLoans = configSafeReader["AutoReApproveMaxNumOfOutstandingLoans"];

				var sr = Db.GetFirst(
					"GetLastOfferDataForReApproval",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId)
				);

				if (sr.IsEmpty) // Cant reapprove without previous approvals
					return false;

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
					var instance = new GetAvailableFunds(Db, log);
					instance.Execute();
					decimal availableFunds = instance.AvailableFunds;

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

						response.CreditResult = "Approved";
						response.UserStatus = "Approved";
						response.SystemDecision = "Approve";
						response.LoanOfferUnderwriterComment = "Auto Re-Approval";
						response.DecisionName = "Re-Approval";
						response.AppValidFor = DateTime.UtcNow.AddDays((loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays);
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
			catch (Exception e)
			{
				log.Error("Exception during reapproval:{0}", e);
				return false;
			}
		}
	}
}
