namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class TodayApprovalCount : AThresholdTrace {
		public TodayApprovalCount(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "today approval count"; }
		} // ValueName
	}  // class TodayApprovalCount
} // namespace
