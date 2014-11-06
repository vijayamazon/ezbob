namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions {
	using System;
	using EzBob.Backend.Strategies.Misc;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ReApproval {
		public ReApproval(int customerId, decimal loanOfferReApprovalFullAmount, decimal loanOfferReApprovalRemainingAmount, decimal loanOfferReApprovalFullAmountOld, decimal loanOfferReApprovalRemainingAmountOld, AConnection oDb, ASafeLog oLog) {
			Db = oDb;
			log = oLog;

			this.customerId = customerId;

			strategyHelper = new StrategyHelper();

			this.loanOfferReApprovalFullAmount = loanOfferReApprovalFullAmount;
			this.loanOfferReApprovalRemainingAmount = loanOfferReApprovalRemainingAmount;
			this.loanOfferReApprovalFullAmountOld = loanOfferReApprovalFullAmountOld;
			this.loanOfferReApprovalRemainingAmountOld = loanOfferReApprovalRemainingAmountOld;
		} // constructor

		public void MakeDecision(AutoDecisionResponse response) {
			try {
				var configSafeReader = Db.GetFirst("GetReApprovalConfigs", CommandSpecies.StoredProcedure);
				int autoReApproveMaxNumOfOutstandingLoans = configSafeReader["AutoReApproveMaxNumOfOutstandingLoans"];

				var sr = Db.GetFirst(
					"GetLastOfferDataForReApproval",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId)
				);

				if (sr.IsEmpty) // Cant reapprove without previous approvals
					return;

				bool loanOfferEmailSendingBanned = sr["EmailSendingBanned"];
				DateTime loanOfferOfferStart = sr["OfferStart"];
				DateTime loanOfferOfferValidUntil = sr["OfferValidUntil"];
				int loanOfferSystemCalculatedSum = sr["SystemCalculatedSum"];
				int loanOfferSumOfChargesOld = sr["SumOfChargesOld"];
				int loanOfferNumOfMPsAddedOld = sr["NumOfMPsAddedOld"];
				decimal loanOfferPrincipalPaidAmountOld = sr["PrincipalPaidAmountOld"];

				if (
					(
						loanOfferReApprovalFullAmount > 0 ||
						loanOfferReApprovalRemainingAmount > 0
					) || (
						(
							loanOfferReApprovalFullAmountOld > 0 ||
							loanOfferReApprovalRemainingAmountOld > 0
						) &&
						loanOfferPrincipalPaidAmountOld == 0 &&
						loanOfferSumOfChargesOld == 0 &&
						loanOfferNumOfMPsAddedOld == 0
					)
				) {
					var availFunds = new GetAvailableFunds(Db, log);
					availFunds.Execute();

					if (availFunds.AvailableFunds > loanOfferSystemCalculatedSum) {
						int numOfOutstandingLoans = strategyHelper.GetOutstandingLoansNum(customerId);

						if (numOfOutstandingLoans > autoReApproveMaxNumOfOutstandingLoans) {
							response.CreditResult = "WaitingForDecision";
							response.UserStatus = "Manual";
							response.SystemDecision = "Manual";
						}
						else {
							response.CreditResult = "Approved";
							response.UserStatus = "Approved";
							response.SystemDecision = "Approve";
							response.LoanOfferUnderwriterComment = "Auto Re-Approval";
							response.DecisionName = "Re-Approval";
							response.AppValidFor = DateTime.UtcNow.AddDays((loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays);
							response.LoanOfferEmailSendingBannedNew = loanOfferEmailSendingBanned;
						} // if
					}
					else {
						response.CreditResult = "WaitingForDecision";
						response.UserStatus = "Manual";
						response.SystemDecision = "Manual";
					} // if
				} // if
			}
			catch (Exception e) {
				log.Error(e, "Exception during re-approval.");
			} // try
		} // MakeDecision

		private readonly StrategyHelper strategyHelper;

		private readonly AConnection Db;
		private readonly ASafeLog log;

		private readonly int customerId;

		private readonly decimal loanOfferReApprovalFullAmount;
		private readonly decimal loanOfferReApprovalRemainingAmount;
		private readonly decimal loanOfferReApprovalFullAmountOld;
		private readonly decimal loanOfferReApprovalRemainingAmountOld;
	} // class ReApproval
} // namespace
