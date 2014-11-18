namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class EnoughFunds : ATrace {
		public EnoughFunds(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(decimal nApprovedAmount, decimal nAvailableFunds) {
			ApprovedAmount = nApprovedAmount;
			AvailableFunds = nAvailableFunds;

			Comment = string.Format(
				"approved amount is {0}, available funds is {1}",
				ApprovedAmount.ToString("N2"),
				AvailableFunds.ToString("N2")
			);
		} // Init

		public decimal ApprovedAmount { get; private set; }
		public decimal AvailableFunds { get; private set; }
	}  // class EnoughFunds
} // namespace
