namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class TodayApprovalCount : AThresholdTrace {
		public TodayApprovalCount(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "today approval count"; }
		} // ValueName
	}  // class TodayApprovalCount
} // namespace
