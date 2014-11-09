namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class ReduceOutstandingPrincipal : ATrace {
		public ReduceOutstandingPrincipal(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public void Init(decimal nOutstandingPrincipal, int nNewAutoApprovedAmount) {
			OutstandingPrincipal = nOutstandingPrincipal;
			NewAutoApprovedAmount = nNewAutoApprovedAmount;

			Comment = string.Format(
				"customer {0}: after reducing outstanding principal of {1} approved amount is {2}",
				CustomerID,
				OutstandingPrincipal,
				NewAutoApprovedAmount
			);
		} // Init

		public decimal OutstandingPrincipal { get; private set; }
		public int NewAutoApprovedAmount { get; private set; }
	}  // class ReduceOutstandingPrincipal
} // namespace
