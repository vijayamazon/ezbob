﻿namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using Ezbob.Database;
	using System.Data;
	using Ezbob.Logger;

	public class ReRejection
	{
		private readonly bool enableAutomaticReRejection;
		private readonly AConnection Db;
		private readonly ASafeLog log;
		private readonly int customerId;

		public ReRejection(int customerId, bool enableAutomaticReRejection, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.enableAutomaticReRejection = enableAutomaticReRejection;
			this.customerId = customerId;
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				DataTable dt = Db.ExecuteReader("GetCustomerDataForReRejection", 
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("Now", DateTime.UtcNow));
				var sr = new SafeReader(dt.Rows[0]);
				int newCustomerReReject = sr["NewCustomer_ReReject"];
				int oldCustomerReReject = sr["OldCustomer_ReReject"];
				decimal principalPaidAmount = sr["PrincipalPaidAmount"];
				decimal loanAmountTaken = sr["LoanAmountTaken"];

				if (newCustomerReReject > 0 || (oldCustomerReReject > 0 && loanAmountTaken * 0.5m >= principalPaidAmount))
				{
					Rejection.FillPayPalFiguresForExplanationMail(Db, customerId, response);
					response.IsReRejected = true;
					response.AutoRejectReason = "Auto Re-Reject";

					response.CreditResult = enableAutomaticReRejection ? "Rejected" : "WaitingForDecision";

					response.UserStatus = "Rejected";
					response.SystemDecision = "Reject";
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
