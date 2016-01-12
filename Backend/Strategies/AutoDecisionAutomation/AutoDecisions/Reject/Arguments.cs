namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	using System;

	public class Arguments {
		public Arguments(int nCustomerID, long? cashRequestID, DateTime? now = null) {
			CustomerID = nCustomerID;
			CashRequestID = cashRequestID;
			Now = now ?? DateTime.UtcNow;
		} // constructor

		public int CustomerID { get; private set; }
		public long? CashRequestID { get; private set; }
		public DateTime Now { get; private set; }
	} // Arguments
} // namespace
