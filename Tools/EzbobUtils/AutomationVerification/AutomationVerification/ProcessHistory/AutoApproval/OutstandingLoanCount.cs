namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingLoanCount : AThresholdTrace {
		public OutstandingLoanCount(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "outstanding loan count"; }
		} // ValueName
	}  // class OutstandingLoanCount
} // namespace
