namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class DefaultAccounts : ABoolTrace {
		public DefaultAccounts(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string PropertyName {
			get { return "default accounts"; }
		} // PropertyName
	}  // class DefaultAccounts
} // namespace
