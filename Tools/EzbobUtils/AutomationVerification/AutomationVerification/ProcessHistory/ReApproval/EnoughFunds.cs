namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class EnoughFunds : ATrace {
		public EnoughFunds(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public void Init(decimal nApprovedAmount, decimal nAvailableFunds) {
			ApprovedAmount = nApprovedAmount;
			AvailableFunds = nAvailableFunds;

			Comment = string.Format(
				"customer {0} approved amount is {1}, available funds is {2}",
				CustomerID,
				ApprovedAmount.ToString("N2"),
				AvailableFunds.ToString("N2")
			);
		} // Init

		public decimal ApprovedAmount { get; private set; }
		public decimal AvailableFunds { get; private set; }
	}  // class EnoughFunds
} // namespace
