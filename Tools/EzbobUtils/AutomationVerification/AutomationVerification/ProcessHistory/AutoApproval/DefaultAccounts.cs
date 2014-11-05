namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class DefaultAccounts : ATrace {
		public DefaultAccounts(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public void Init() {
			HasDefaultAccounts = !CompletedSuccessfully;

			Comment = string.Format("customer {0} has {1}default accounts", CustomerID, HasDefaultAccounts ? string.Empty : "no ");
		} // Init

		public bool HasDefaultAccounts { get; private set; }
	}  // class DefaultAccounts
} // namespace
