namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class TodayLoans : BusinessScore {
		public TodayLoans(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ScoreName {
			get { return "today loans"; }
		} // ScoreName
	}  // class TodayLoans
} // namespace
