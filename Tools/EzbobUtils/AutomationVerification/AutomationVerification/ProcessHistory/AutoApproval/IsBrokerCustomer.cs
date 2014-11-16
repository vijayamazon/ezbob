namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class IsBrokerCustomer : ABoolTrace {
		public IsBrokerCustomer(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string PropertyName {
			get { return "attachment to broker"; }
		} // PropertyName
	}  // class IsBrokerCustomer
} // namespace
