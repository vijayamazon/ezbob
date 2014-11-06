namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class TodayApprovalCount : BusinessScore {
		public TodayApprovalCount(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ScoreName {
			get { return "today approval count"; }
		} // ScoreName
	}  // class TodayApprovalCount
} // namespace
