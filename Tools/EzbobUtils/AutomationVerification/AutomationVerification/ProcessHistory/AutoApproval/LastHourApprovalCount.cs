namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class LastHourApprovalCount : AThresholdTrace {
		public LastHourApprovalCount(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "Last hour approval count"; }
		} // ValueName
	}  // class LastHourApprovalCount
} // namespace
