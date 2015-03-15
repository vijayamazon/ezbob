namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	public class Arguments {

		public Arguments(int nCustomerID) {
			CustomerID = nCustomerID;
		} // constructor

		public int CustomerID { get; private set; }
	} // Arguments
} // namespace
