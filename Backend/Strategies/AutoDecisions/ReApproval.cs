namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using System;
	using System.Data;
	using Ezbob.Logger;
	using Models;

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
			int loanOfferSystemCalculatedSum = 0;
			if (!int.TryParse(results["SystemCalculatedSum"].ToString(), out loanOfferSystemCalculatedSum))
			{
				log.Debug("The parameter 'SystemCalculatedSum' was null, will use 0.");
			}
			int loanOfferSumOfChargesOld = 0;
			if (!int.TryParse(results["SumOfChargesOld"].ToString(), out loanOfferSumOfChargesOld))
			{
				log.Debug("The parameter 'SumOfChargesOld' was null, will use 0.");
			}
			int loanOfferNumOfMPsAddedOld = 0;
			if (!int.TryParse(results["NumOfMPsAddedOld"].ToString(), out loanOfferNumOfMPsAddedOld))
			{
				log.Debug("The parameter 'NumOfMPsAddedOld' was null, will use 0.");
			}
			decimal loanOfferPrincipalPaidAmountOld = 0;
			if (!decimal.TryParse(results["PrincipalPaidAmountOld"].ToString(), out loanOfferPrincipalPaidAmountOld))
			{
				log.Debug("The parameter 'PrincipalPaidAmountOld' was null, will use 0.");
			}

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
