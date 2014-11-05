namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Turnover : BusinessScore {
		public Turnover(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public string PeriodName { get; set; }

		protected override string ScoreName {
			get { return PeriodName + " turnover"; }
		} // ScoreName
	}  // class Turnover
} // namespace
