namespace Strategies.AutoDecisions
{
	using System;
	using EzBob.Backend.Strategies;
	using EzBob.Models;

	public class ReApproval
	{
		private StrategyHelper strategyHelper = new StrategyHelper();
		public int LoanOffer_SystemCalculatedSum { get; private set; }
		public int LoanOffer_PrincipalPaidAmountOld { get; private set; }
		public int AutoReApproveMaxNumOfOutstandingLoans { get; private set; }
		public bool LoanOffer_EmailSendingBanned { get; private set; }
		public DateTime LoanOffer_OfferStart { get; private set; }
		public DateTime LoanOffer_OfferValidUntil { get; private set; }
		public int LoanOffer_NumOfMPsAddedOld { get; private set; }
		public int LoanOffer_SumOfChargesOld { get; private set; }

		public bool MakeDecision(MainStrategy mainStrategy)
		{
			if ((mainStrategy.LoanOffer_ReApprovalFullAmount > 0 || mainStrategy.LoanOffer_ReApprovalRemainingAmount > 0) ||
			    ((mainStrategy.LoanOffer_ReApprovalFullAmountOld > 0 || mainStrategy.LoanOffer_ReApprovalRemainingAmountOld > 0) &&
			     LoanOffer_PrincipalPaidAmountOld == 0 && LoanOffer_SumOfChargesOld == 0 &&
			     LoanOffer_NumOfMPsAddedOld == 0))
			{
				if (mainStrategy.AvailableFunds > LoanOffer_SystemCalculatedSum)
				{
					mainStrategy.NumOfOutstandingLoans = strategyHelper.GetOutstandingLoansNum(mainStrategy.CustomerId);
					if (mainStrategy.NumOfOutstandingLoans > AutoReApproveMaxNumOfOutstandingLoans)
					{
						mainStrategy.CreditResult = "WaitingForDecision";
						mainStrategy.UserStatus = "Manual";
						mainStrategy.SystemDecision = "Manual";
						return true;
					}

					mainStrategy.CreditResult = mainStrategy.EnableAutomaticReApproval ? "Approved" : "WaitingForDecision";
					mainStrategy.UserStatus = "Approved";
					mainStrategy.SystemDecision = "Approve";
					mainStrategy.LoanOffer_UnderwriterComment = "Auto Re-Approval";
					mainStrategy.LoanOffer_OfferValidDays =
						(LoanOffer_OfferValidUntil - LoanOffer_OfferStart).TotalDays;
					mainStrategy.App_ApplyForLoan = null;
					mainStrategy.App_ValidFor = DateTime.UtcNow.AddDays(mainStrategy.LoanOffer_OfferValidDays);
					mainStrategy.LoanOffer_EmailSendingBanned_new = LoanOffer_EmailSendingBanned;
					return true;
				}


				mainStrategy.CreditResult = "WaitingForDecision";
				mainStrategy.UserStatus = "Manual";
				mainStrategy.SystemDecision = "Manual";
				return true;
			}

			return false;
		}
	}
}
