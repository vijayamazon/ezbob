namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using Backend.Strategies;
	using Models;

	public class Approval
	{
		private StrategyHelper strategyHelper = new StrategyHelper();

		public bool AutoApproveIsSilent { get; private set; }
		public string AutoApproveSilentTemplateName { get; private set; }
		public string AutoApproveSilentToAddress { get; private set; }
		public bool LoanOffer_EmailSendingBanned { get; private set; }
		public DateTime LoanOffer_OfferStart { get; private set; }
		public DateTime LoanOffer_OfferValidUntil { get; private set; }

		public bool MakeDecision(MainStrategy mainStrategy)
		{
			if (mainStrategy.EnableAutomaticApproval)
			{
				mainStrategy.AutoApproveAmount = strategyHelper.AutoApproveCheck(mainStrategy.CustomerId, mainStrategy.OfferedCreditLine, mainStrategy.MinExperianScore);

				if (mainStrategy.AutoApproveAmount != 0)
				{
					mainStrategy.AvailableFunds = 0; // TODO: call GetAvailableFunds

					if (mainStrategy.AvailableFunds > mainStrategy.AutoApproveAmount)
					{
						if (AutoApproveIsSilent)
						{
							strategyHelper.NotifyAutoApproveSilentMode(mainStrategy.CustomerId, mainStrategy.AutoApproveAmount, AutoApproveSilentTemplateName, AutoApproveSilentToAddress);

							mainStrategy.CreditResult = "WaitingForDecision";
							mainStrategy.UserStatus = "Manual";
							mainStrategy.SystemDecision = "Manual";
						}
						else
						{
							mainStrategy.CreditResult = "Approved";
							mainStrategy.UserStatus = "Approved";
							mainStrategy.SystemDecision = "Approve";
							mainStrategy.LoanOffer_UnderwriterComment = "Auto Approval";
							mainStrategy.LoanOffer_OfferValidDays = (LoanOffer_OfferValidUntil - LoanOffer_OfferStart).TotalDays;
							mainStrategy.App_ApplyForLoan = null;
							mainStrategy.App_ValidFor = DateTime.UtcNow.AddDays(mainStrategy.LoanOffer_OfferValidDays);
							mainStrategy.IsAutoApproval = true;
							mainStrategy.LoanOffer_EmailSendingBanned_new = LoanOffer_EmailSendingBanned;
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
