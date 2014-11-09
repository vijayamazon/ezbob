namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingRepayRatio : AThresholdTrace {
		public OutstandingRepayRatio(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "outstanding principal repay ratio"; }
		} // ValueName
	}  // class OutstandingRepayRatio
} // namespace
