namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using System.Data;
	using Ezbob.Logger;

	public class ReRejection
	{
		private readonly int newCustomerReReject;
		private readonly int oldCustomerReReject;
		private readonly decimal principalPaidAmount;
		private readonly decimal loanAmountTaken;
		private readonly AutoDecisionRequest request;
		private readonly AConnection Db;
		private readonly ASafeLog log;

		public ReRejection(AutoDecisionRequest request, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.request = request;
			DataTable dt = Db.ExecuteReader("GetCustomerDataForReRejection", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", request.CustomerId));
			var sr = new SafeReader(dt.Rows[0]);
			newCustomerReReject = sr["NewCustomer_ReReject"];
			oldCustomerReReject = sr["OldCustomer_ReReject"];
			principalPaidAmount = sr["PrincipalPaidAmount"];
			loanAmountTaken = sr["LoanAmountTaken"];
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
				return true;
			}

			return false;
		}
	}
}
