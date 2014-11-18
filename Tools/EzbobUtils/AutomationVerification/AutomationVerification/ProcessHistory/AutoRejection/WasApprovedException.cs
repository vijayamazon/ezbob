namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class WasApproved : ABoolTrace {
		public WasApproved(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string PropertyName {
			get { return "Was approved for a loan"; }
		} // PropertyName
	}  // class 
} // namespace
