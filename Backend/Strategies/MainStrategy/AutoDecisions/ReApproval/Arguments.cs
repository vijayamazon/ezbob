namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval {
	internal class Arguments {
		#region constructor

		public Arguments(int nCustomerID, decimal nMaxApprovalAmount) {
			CustomerID = nCustomerID;
			MaxApprovalAmount = nMaxApprovalAmount;
		} // constructor

		#endregion constructor

		public int CustomerID { get; private set; }
		public decimal MaxApprovalAmount { get; private set; }
	} // Arguments
} // namespace
