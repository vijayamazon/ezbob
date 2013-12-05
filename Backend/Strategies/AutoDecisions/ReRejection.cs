namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System.Data;
	using DbConnection;

	public class ReRejection
	{
		private readonly int newCustomerReReject;
		private readonly int oldCustomerReReject;
		private readonly int principalPaidAmount;
		private readonly int loanAmountTaken;
		private readonly AutoDecisionRequest request;

		public ReRejection(AutoDecisionRequest request)
		{
			this.request = request;
			DataTable dt = DbConnection.ExecuteSpReader("GetCustomerDataForReRejection");
			DataRow results = dt.Rows[0];
			newCustomerReReject = int.Parse(results["NewCustomer_ReReject"].ToString());
			oldCustomerReReject = int.Parse(results["OldCustomer_ReReject"].ToString());
			principalPaidAmount = int.Parse(results["PrincipalPaidAmount"].ToString());
			loanAmountTaken = int.Parse(results["LoanAmountTaken"].ToString());
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (newCustomerReReject > 0 || (oldCustomerReReject > 0 && loanAmountTaken * 0.5 >= principalPaidAmount))
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
