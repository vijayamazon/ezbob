namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class HourlyApprovalCount : AThresholdTrace {
		public HourlyApprovalCount(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "Hourly approval count"; }
		} // ValueName
	}  // class HourlyApprovalCount
} // namespace
