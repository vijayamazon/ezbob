namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	using System;

	public class Arguments {
		public Arguments(int nCustomerID, long? cashRequestID, long? nlCashRequestID, DateTime? now = null) {
			CustomerID = nCustomerID;
			CashRequestID = cashRequestID;
			NLCashRequestID = nlCashRequestID;
			Now = now ?? DateTime.UtcNow;
		} // constructor

		public int CustomerID { get; private set; }
		public long? CashRequestID { get; private set; }
		public long? NLCashRequestID { get; private set; }
		public DateTime Now { get; private set; }
	} // Arguments
} // namespace
