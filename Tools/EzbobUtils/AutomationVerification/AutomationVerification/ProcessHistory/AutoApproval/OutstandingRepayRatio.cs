namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingRepayRatio : BusinessScore {
		public OutstandingRepayRatio(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ScoreName {
			get { return "outstanding principal repay ratio"; }
		} // ScoreName
	}  // class OutstandingRepayRatio
} // namespace
