namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class IsBrokerCustomer : DefaultAccounts {
		public IsBrokerCustomer(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string PropertyName {
			get { return "attachment to broker"; }
		} // PropertyName
	}  // class IsBrokerCustomer
} // namespace
