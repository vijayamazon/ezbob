namespace EzBob.Backend.Strategies.AutoDecisions
{
	public class ReRejection
	{
		public int Re_Reject_NewCustomer_ReReject { get; private set; }
		public int Re_Reject_OldCustomer_ReReject { get; private set; }
		public int Re_Reject_PrincipalPaidAmount { get; private set; }
		public int Re_Reject_LoanAmountTaken { get; private set; }

		public bool MakeDecision(AutoDecisionRequest request, AutoDecisionResponse response)
		{
			if (Re_Reject_NewCustomer_ReReject > 0 || (Re_Reject_OldCustomer_ReReject > 0 && Re_Reject_LoanAmountTaken * 0.5 >= Re_Reject_PrincipalPaidAmount))
			{
				response.IsReRejected = true;
				response.AutoRejectReason = "Auto Re-Reject";

				response.CreditResult = request.EnableAutomaticReRejection ? "Rejected" : "WaitingForDecision";

				response.UserStatus = "Rejected";
				response.SystemDecision = "Reject";
				response.ModelLoanOffer = 0;
				return true;
			}

			return false;
		}
	}
}
