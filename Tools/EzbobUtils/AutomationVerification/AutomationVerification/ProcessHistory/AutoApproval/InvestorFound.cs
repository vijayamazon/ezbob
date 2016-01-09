namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class InvestorFound : ABoolTrace {
		public InvestorFound(DecisionStatus status) : base(status) {
		} // constructor

		protected override string PropertyName {
			get { return "matching investor"; }
		} // PropertyName
	} // class InvestorFound
} // namespace
