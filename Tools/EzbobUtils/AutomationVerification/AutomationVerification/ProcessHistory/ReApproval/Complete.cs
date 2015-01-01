namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class Complete : AThresholdTrace {
		public Complete(DecisionStatus nDecisionStatus)
			: base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "Approved amount"; }
		}
	}  // class Complete
} // namespace
