namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	public class Arguments {

		public Arguments(int nCustomerID, long? cashRequestID, long? nlCashRequestID) {
			CustomerID = nCustomerID;
			CashRequestID = cashRequestID;
			NLCashRequestID = nlCashRequestID;
		} // constructor

		public int CustomerID { get; private set; }
		public long? CashRequestID { get; private set; }
		public long? NLCashRequestID { get; private set; }
	} // Arguments
} // namespace
