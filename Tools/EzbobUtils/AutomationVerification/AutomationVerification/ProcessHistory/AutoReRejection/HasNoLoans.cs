namespace AutomationCalculator.ProcessHistory.AutoReRejection {
	public class HasNoLoans : ABoolTrace {
		public HasNoLoans(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		protected override string PropertyName {
			get { return "loans"; }
		} // PropertyName
	}  // class 
} // namespace
