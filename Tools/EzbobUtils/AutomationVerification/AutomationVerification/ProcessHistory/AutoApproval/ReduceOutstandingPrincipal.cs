namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class ReduceOutstandingPrincipal : ATrace {
		public ReduceOutstandingPrincipal(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(decimal nOutstandingPrincipal, decimal nNewAutoApprovedAmount) {
			OutstandingPrincipal = nOutstandingPrincipal;
			NewAutoApprovedAmount = nNewAutoApprovedAmount;

			Comment = string.Format(
				"after reducing outstanding principal of {0} approved amount is {1}",
				OutstandingPrincipal,
				NewAutoApprovedAmount
			);
		} // Init

		public decimal OutstandingPrincipal { get; private set; }
		public decimal NewAutoApprovedAmount { get; private set; }
	}  // class ReduceOutstandingPrincipal
} // namespace
