namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OnePricingFound : ABoolTrace {
		public OnePricingFound(DecisionStatus status) : base(status) {
		} // constructor

		protected override string PropertyName {
			get { return "one matching pricing configuration"; }
		} // PropertyName
	} // class OnePricingFound
} // namespace
