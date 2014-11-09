namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class TodayApprovalCount : AThresholdTrace {
		public TodayApprovalCount(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "today approval count"; }
		} // ValueName
	}  // class TodayApprovalCount
} // namespace
