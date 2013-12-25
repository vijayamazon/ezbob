namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using System;
	using System.Data;
	using Models;

	public class Approval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly AutoDecisionRequest request;
		private readonly bool autoApproveIsSilent;
		private readonly string autoApproveSilentTemplateName;
		private readonly string autoApproveSilentToAddress;
		private AConnection Db { get; set; }

		public Approval(AutoDecisionRequest request, AConnection oDb) {
			Db = oDb;
			this.request = request;
			DataTable dt = Db.ExecuteReader("GetApprovalConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];

			autoApproveIsSilent = Convert.ToBoolean(results["AutoApproveIsSilent"]);
			autoApproveSilentTemplateName = results["AutoApproveSilentTemplateName"].ToString();
			autoApproveSilentToAddress = results["AutoApproveSilentToAddress"].ToString();
		} // constructor

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (request.EnableAutomaticApproval)
			{
				response.AutoApproveAmount = strategyHelper.AutoApproveCheck(request.CustomerId, request.OfferedCreditLine, request.MinExperianScore);

				if (response.AutoApproveAmount != 0)
				{
					DataTable dt = Db.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure);
					decimal availableFunds = decimal.Parse(dt.Rows[0]["AvailableFunds"].ToString());

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

							DataRow results = dt.Rows[0];
							bool loanOfferEmailSendingBanned = Convert.ToBoolean(results["EmailSendingBanned"]);
							DateTime loanOfferOfferStart = DateTime.Parse(results["OfferStart"].ToString());
							DateTime loanOfferOfferValidUntil = DateTime.Parse(results["OfferValidUntil"].ToString());

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
