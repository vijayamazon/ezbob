namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class Charges : ATrace {
		#region constructor

		public Charges(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		#endregion constructor

		public void Init(decimal nAmount) {
			Amount = nAmount;

			if (CompletedSuccessfully) {
				Comment = string.Format(
					"customer {0} has no charges after the last manually approved cash request",
					CustomerID
					);
			}
			else {
				Comment = string.Format(
					"customer {0} was charged for {1} after the last manually approved cash request",
					CustomerID,
					Amount.ToString("N2")
				);
			} // if
		} // Init

		public decimal Amount { get; private set; }
	} // class Charges
} // namespace
