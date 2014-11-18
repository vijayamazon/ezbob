namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class IsBrokerCustomer : ABoolTrace {
		public IsBrokerCustomer(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string PropertyName {
			get { return "attachment to broker"; }
		} // PropertyName
	}  // class IsBrokerCustomer
} // namespace
