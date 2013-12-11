namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Data;
	using DbConnection;
	using Models;

	public class Approval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly AutoDecisionRequest request;
		private readonly bool autoApproveIsSilent;
		private readonly string autoApproveSilentTemplateName;
		private readonly string autoApproveSilentToAddress;

		public Approval(AutoDecisionRequest request)
		{
			this.request = request;
			DataTable dt = DbConnection.ExecuteSpReader("GetApprovalConfigs");
			DataRow results = dt.Rows[0];

			autoApproveIsSilent = bool.Parse(results["AutoApproveIsSilent"].ToString());
			autoApproveSilentTemplateName = results["AutoApproveSilentTemplateName"].ToString();
			autoApproveSilentToAddress = results["AutoApproveSilentToAddress"].ToString();
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (request.EnableAutomaticApproval)
			{
				response.AutoApproveAmount = strategyHelper.AutoApproveCheck(request.CustomerId, request.OfferedCreditLine, request.MinExperianScore);

				if (response.AutoApproveAmount != 0)
				{
					DataTable dt = DbConnection.ExecuteSpReader("GetAvailableFunds");
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
							dt = DbConnection.ExecuteSpReader("GetLastOfferDataForApproval", DbConnection.CreateParam("CustomerId", request.CustomerId));
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
							response.App_ValidFor = DateTime.UtcNow.AddDays(request.LoanOffer_OfferValidDays);
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
