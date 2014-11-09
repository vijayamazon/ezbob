namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class DefaultAccounts : ABoolTrace {
		public DefaultAccounts(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string PropertyName {
			get { return "default accounts"; }
		} // PropertyName
	}  // class DefaultAccounts
} // namespace
