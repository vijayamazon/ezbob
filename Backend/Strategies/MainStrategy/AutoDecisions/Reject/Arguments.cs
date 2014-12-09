namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
	public class Arguments {

		public Arguments(int nCustomerID) {
			CustomerID = nCustomerID;
		} // constructor

		public int CustomerID { get; private set; }
	} // Arguments
} // namespace
