namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ReRejection
	{
		private readonly AConnection Db;
		private readonly ASafeLog log;
		private readonly int customerId;

		public ReRejection(int customerId, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.customerId = customerId;
		}

		public bool MakeDecision(AutoDecisionRejectionResponse response)
		{
			try
			{
				SafeReader sr = Db.GetFirst("GetCustomerDataForReRejection", 
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("Now", DateTime.UtcNow));

				int newCustomerReReject = sr["NewCustomer_ReReject"];
				int oldCustomerReReject = sr["OldCustomer_ReReject"];
				decimal principalPaidAmount = sr["PrincipalPaidAmount"];
				decimal loanAmountTaken = sr["LoanAmountTaken"];

				if (newCustomerReReject > 0 || (oldCustomerReReject > 0 && loanAmountTaken * 0.5m >= principalPaidAmount))
				{
					response.IsReRejected = true;
					response.AutoRejectReason = "Auto Re-Reject";

					response.CreditResult = "Rejected";

					response.UserStatus = "Rejected";
					response.SystemDecision = "Reject";
					response.DecidedToReject = true;
					response.DecisionName = "Re-rejection";

					return true;
				}

				return false;
			}
			catch (Exception e)
			{
				log.Error("Exception during rerejection:{0}", e);
				return false;
			}
		}
	}
}
