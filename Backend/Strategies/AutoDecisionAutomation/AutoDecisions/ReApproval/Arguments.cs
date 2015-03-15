namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval {
	public class Arguments {

		public Arguments(int nCustomerID) {
			CustomerID = nCustomerID;
		} // constructor

		public int CustomerID { get; private set; }
	} // Arguments
} // namespace
