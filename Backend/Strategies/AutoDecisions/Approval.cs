using Ezbob.Database;

namespace EzBob.Backend.Strategies.AutoDecisions
{
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
		private AConnection DB { get; set; }

		public Approval(AutoDecisionRequest request, AConnection oDB) {
			DB = oDB;
			this.request = request;
			DataTable dt = DB.ExecuteReader("GetApprovalConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];

			autoApproveIsSilent = bool.Parse(results["AutoApproveIsSilent"].ToString());
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
					DataTable dt = DB.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure);
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
							dt = DB.ExecuteReader(
								"GetLastOfferDataForApproval",
								CommandSpecies.StoredProcedure,
								new QueryParameter("CustomerId", request.CustomerId)
							);

							DataRow results = dt.Rows[0];
							bool loanOfferEmailSendingBanned = bool.Parse(results["EmailSendingBanned"].ToString());
							DateTime loanOfferOfferStart = DateTime.Parse(results["OfferStart"].ToString());
							DateTime loanOfferOfferValidUntil = DateTime.Parse(results["OfferValidUntil"].ToString());

							response.CreditResult = "Approved";
							response.UserStatus = "Approved";
							response.SystemDecision = "Approve";
							response.LoanOffer_UnderwriterComment = "Auto Approval";
							response.LoanOffer_OfferValidDays = (loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays;
							response.App_ApplyForLoan = null;
							response.App_ValidFor = DateTime.UtcNow.AddDays(response.LoanOffer_OfferValidDays);
							response.IsAutoApproval = true;
							response.LoanOffer_EmailSendingBanned_new = loanOfferEmailSendingBanned;
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
