namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval {
	public class Arguments {

		public Arguments(int nCustomerID, long? cashRequestID) {
			CustomerID = nCustomerID;
			CashRequestID = cashRequestID;
		} // constructor

		public int CustomerID { get; private set; }
		public long? CashRequestID { get; private set; }
	} // Arguments
} // namespace
