namespace Strategies.AutoDecisions
{
	using EzBob.Backend.Strategies;

	public class ReRejection
	{
		public int Re_Reject_NewCustomer_ReReject { get; private set; }
		public int Re_Reject_OldCustomer_ReReject { get; private set; }
		public int Re_Reject_PrincipalPaidAmount { get; private set; }
		public int Re_Reject_LoanAmountTaken { get; private set; }

		public bool MakeDecision(MainStrategy mainStrategy)
		{
			if (Re_Reject_NewCustomer_ReReject > 0 || (Re_Reject_OldCustomer_ReReject > 0 && Re_Reject_LoanAmountTaken * 0.5 >= Re_Reject_PrincipalPaidAmount))
			{
				mainStrategy.IsReRejected = true;
				mainStrategy.AutoRejectReason = "Auto Re-Reject";

				mainStrategy.CreditResult = mainStrategy.EnableAutomaticReRejection ? "Rejected" : "WaitingForDecision";

				mainStrategy.UserStatus = "Rejected";
				mainStrategy.SystemDecision = "Reject";
				mainStrategy.ModelLoanOffer = 0;
				return true;
			}

			return false;
		}
	}
}
