namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Data;
	using DbConnection;
	using Models;

	public class Approval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();

		public Approval()
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetApprovalConfigs");
			DataRow results = dt.Rows[0];

			autoApproveIsSilent = bool.Parse(results["AutoApproveIsSilent"].ToString());
			autoApproveSilentTemplateName = results["AutoApproveSilentTemplateName"].ToString();
			autoApproveSilentToAddress = results["AutoApproveSilentToAddress"].ToString();
		}

		private readonly bool autoApproveIsSilent;
		private readonly string autoApproveSilentTemplateName;
		private readonly string autoApproveSilentToAddress;
		private int autoApproveAmount;
		private decimal availableFunds;

		public bool MakeDecision(AutoDecisionRequest request, AutoDecisionResponse response)
		{
			if (request.EnableAutomaticApproval)
			{
				autoApproveAmount = strategyHelper.AutoApproveCheck(request.CustomerId, request.OfferedCreditLine, request.MinExperianScore);

				if (autoApproveAmount != 0)
				{
					DataTable dt = DbConnection.ExecuteSpReader("GetAvailableFunds");
					availableFunds = decimal.Parse(dt.Rows[0]["AvailableFunds"].ToString());

					if (availableFunds > autoApproveAmount)
					{
						if (autoApproveIsSilent)
						{
							strategyHelper.NotifyAutoApproveSilentMode(request.CustomerId, autoApproveAmount, autoApproveSilentTemplateName, autoApproveSilentToAddress);

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
