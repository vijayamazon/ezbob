namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Data;
	using Backend.Strategies;
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

		public bool MakeDecision(MainStrategy mainStrategy)
		{
			if (mainStrategy.EnableAutomaticApproval)
			{
				autoApproveAmount = strategyHelper.AutoApproveCheck(mainStrategy.CustomerId, mainStrategy.OfferedCreditLine, mainStrategy.MinExperianScore);

				if (autoApproveAmount != 0)
				{
					DataTable dt = DbConnection.ExecuteSpReader("GetAvailableFunds");
					availableFunds = decimal.Parse(dt.Rows[0]["AvailableFunds"].ToString());

					if (availableFunds > autoApproveAmount)
					{
						if (autoApproveIsSilent)
						{
							strategyHelper.NotifyAutoApproveSilentMode(mainStrategy.CustomerId, autoApproveAmount, autoApproveSilentTemplateName, autoApproveSilentToAddress);

							mainStrategy.CreditResult = "WaitingForDecision";
							mainStrategy.UserStatus = "Manual";
							mainStrategy.SystemDecision = "Manual";
						}
						else
						{
							dt = DbConnection.ExecuteSpReader("GetOfferDatesForApproval");
							DataRow results = dt.Rows[0];
							bool loanOfferEmailSendingBanned = bool.Parse(results["EmailSendingBanned"].ToString());
							DateTime loanOfferOfferStart = DateTime.Parse(results["OfferStart"].ToString());
							DateTime loanOfferOfferValidUntil = DateTime.Parse(results["OfferValidUntil"].ToString());

							mainStrategy.CreditResult = "Approved";
							mainStrategy.UserStatus = "Approved";
							mainStrategy.SystemDecision = "Approve";
							mainStrategy.LoanOffer_UnderwriterComment = "Auto Approval";
							mainStrategy.LoanOffer_OfferValidDays = (loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays;
							mainStrategy.App_ApplyForLoan = null;
							mainStrategy.App_ValidFor = DateTime.UtcNow.AddDays(mainStrategy.LoanOffer_OfferValidDays);
							mainStrategy.IsAutoApproval = true;
							mainStrategy.LoanOffer_EmailSendingBanned_new = loanOfferEmailSendingBanned;
						}
					}
					else
					{
						mainStrategy.CreditResult = "WaitingForDecision";
						mainStrategy.UserStatus = "Manual";
						mainStrategy.SystemDecision = "Manual";
					}

					return true;
				}
			}

			return false;
		}
	}
}
