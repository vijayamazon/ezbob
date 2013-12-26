namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using System.Data;

	public class ReRejection
	{
		private readonly int newCustomerReReject;
		private readonly int oldCustomerReReject;
		private readonly decimal principalPaidAmount;
		private readonly decimal loanAmountTaken;
		private readonly AutoDecisionRequest request;
		private AConnection Db { get; set; }

		public ReRejection(AutoDecisionRequest request, AConnection oDb) {
			Db = oDb;
			this.request = request;
			DataTable dt = Db.ExecuteReader("GetCustomerDataForReRejection", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", request.CustomerId));
			DataRow results = dt.Rows[0];
			newCustomerReReject = int.Parse(results["NewCustomer_ReReject"].ToString());
			oldCustomerReReject = int.Parse(results["OldCustomer_ReReject"].ToString());
			principalPaidAmount = decimal.Parse(results["PrincipalPaidAmount"].ToString());
			loanAmountTaken = decimal.Parse(results["LoanAmountTaken"].ToString());
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (newCustomerReReject > 0 || (oldCustomerReReject > 0 && loanAmountTaken * 0.5m >= principalPaidAmount))
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
