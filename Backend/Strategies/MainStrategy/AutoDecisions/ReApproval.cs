namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions {
	using System;
	using System.Reflection;
	using EzBob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ReApproval {
		public ReApproval(int customerId, decimal nMaxApprovalAmount, AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog;

			m_nCustomerID = customerId;
			m_nMaxApprovalAmount = nMaxApprovalAmount;
		} // constructor

		public void MakeDecision(AutoDecisionResponse response) {
			try {
				var cfg = new Cfg(m_oDB);

				/*

				var sr = Db.GetFirst(
					"GetLastOfferDataForReApproval",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId)
				);

				bool loanOfferEmailSendingBanned = sr["EmailSendingBanned"];
				DateTime loanOfferOfferStart = sr["OfferStart"];
				DateTime loanOfferOfferValidUntil = sr["OfferValidUntil"];
				int loanOfferSystemCalculatedSum = sr["SystemCalculatedSum"];
				int loanOfferSumOfChargesOld = sr["SumOfChargesOld"];
				int loanOfferNumOfMPsAddedOld = sr["NumOfMPsAddedOld"];
				decimal loanOfferPrincipalPaidAmountOld = sr["PrincipalPaidAmountOld"];

				var availFunds = new GetAvailableFunds(Db, log);
				availFunds.Execute();


				if (approved) {
					response.CreditResult = "Approved";
					response.UserStatus = "Approved";
					response.SystemDecision = "Approve";
					response.LoanOfferUnderwriterComment = "Auto Re-Approval";
					response.DecisionName = "Re-Approval";
					response.AppValidFor = DateTime.UtcNow.AddDays((loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays);
					response.LoanOfferEmailSendingBannedNew = loanOfferEmailSendingBanned;
				} // if
				*/
			}
			catch (Exception e) {
				m_oLog.Error(e, "Exception during re-approval.");
			} // try
		} // MakeDecision

		private class Cfg {
			public Cfg(AConnection oDB) {
				oDB.ForEachRowSafe(SetValue, "GetReApprovalConfigs", CommandSpecies.StoredProcedure);
			} // constructor

			public int AutoReApproveMaxLacrAge { get; private set; }
			public int AutoReApproveMaxLatePayment { get; private set; }
			public int AutoReApproveMaxNumOfOutstandingLoans { get; private set; }

			private void SetValue(SafeReader sr) {
				string sName = sr["Name"];

				PropertyInfo pi = this.GetType().GetProperty(sName);

				if (pi == null)
					return;

				pi.SetValue(this, sr["Value"].ToType(pi.PropertyType));
			} // SetValue
		} // Cfg

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private readonly int m_nCustomerID;
		private readonly decimal m_nMaxApprovalAmount;
	} // class ReApproval
} // namespace
