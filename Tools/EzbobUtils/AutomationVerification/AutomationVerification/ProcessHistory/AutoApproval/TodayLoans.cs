namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class TodayLoans : AThresholdTrace {
		public TodayLoans(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "today loans"; }
		} // ValueName
	}  // class TodayLoans
} // namespace
