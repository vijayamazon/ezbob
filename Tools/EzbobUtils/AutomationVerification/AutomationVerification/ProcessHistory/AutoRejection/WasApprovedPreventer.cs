namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class WasApprovedPreventer : ABoolTrace {
		public WasApprovedPreventer(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string PropertyName {
			get { return "Was approved for a loan"; }
		} // PropertyName
	}  // class 
} // namespace
