namespace EzBob.Backend.Strategies.AutoDecisions
{
	using EzBob.Models;
	using Ezbob.Database;
	using System;
	using System.Data;
	using Ezbob.Logger;

	public class Approval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly AutoDecisionRequest request;
		private readonly bool autoApproveIsSilent;
		private readonly string autoApproveSilentTemplateName;
		private readonly string autoApproveSilentToAddress;
		private readonly AConnection Db;
		private readonly ASafeLog log;

		public Approval(AutoDecisionRequest request, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.request = request;
			DataTable dt = Db.ExecuteReader("GetApprovalConfigs", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(dt.Rows[0]);

			autoApproveIsSilent = sr["AutoApproveIsSilent"];
			autoApproveSilentTemplateName = sr["AutoApproveSilentTemplateName"];
			autoApproveSilentToAddress = sr["AutoApproveSilentToAddress"];
		} // constructor

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (request.EnableAutomaticApproval)
			{
				response.AutoApproveAmount = strategyHelper.AutoApproveCheck(request.CustomerId, request.OfferedCreditLine, request.MinExperianScore);

				if (response.AutoApproveAmount != 0)
				{
					DataTable dt = Db.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure);
					var sr = new SafeReader(dt.Rows[0]);
					decimal availableFunds = sr["AvailableFunds"];

					if (availableFunds > response.AutoApproveAmount)
					{
						if (autoApproveIsSilent)
						{
							strategyHelper.NotifyAutoApproveSilentMode(request.CustomerId, response.AutoApproveAmount, autoApproveSilentTemplateName, autoApproveSilentToAddress);

							response.CreditResult = "WaitingForDecision";
							response.UserStatus = "Manual";
							response.SystemDecision = "Manual";
						}
						else
						{
							dt = Db.ExecuteReader(
								"GetLastOfferDataForApproval",
								CommandSpecies.StoredProcedure,
								new QueryParameter("CustomerId", request.CustomerId)
							);

							sr = new SafeReader(dt.Rows[0]);
							bool loanOfferEmailSendingBanned = sr["EmailSendingBanned"];
							DateTime loanOfferOfferStart = sr["OfferStart"];
							DateTime loanOfferOfferValidUntil = sr["OfferValidUntil"];

							response.CreditResult = "Approved";
							response.UserStatus = "Approved";
							response.SystemDecision = "Approve";
							response.LoanOfferUnderwriterComment = "Auto Approval";
							response.LoanOfferOfferValidDays = (loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays;
							response.AppApplyForLoan = null;
							response.AppValidFor = DateTime.UtcNow.AddDays(response.LoanOfferOfferValidDays);
							response.IsAutoApproval = true;
							response.LoanOfferEmailSendingBannedNew = loanOfferEmailSendingBanned;
						}
					}
					else
					{
						response.CreditResult = "WaitingForDecision";
						response.UserStatus = "Manual";
						response.SystemDecision = "Manual";
					}

					return true;
				}
			}

			return false;
		}
	}
}
