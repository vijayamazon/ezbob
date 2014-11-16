namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class DefaultAccounts : ABoolTrace {
		public DefaultAccounts(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string PropertyName {
			get { return "default accounts"; }
		} // PropertyName
	}  // class DefaultAccounts
} // namespace
