namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval {
	public class Arguments {
		#region constructor

		public Arguments(int nCustomerID) {
			CustomerID = nCustomerID;
		} // constructor

		#endregion constructor

		public int CustomerID { get; private set; }
	} // Arguments
} // namespace
