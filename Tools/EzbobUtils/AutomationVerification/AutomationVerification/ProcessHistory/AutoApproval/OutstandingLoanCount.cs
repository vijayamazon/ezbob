namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingLoanCount : BusinessScore {
		public OutstandingLoanCount(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ScoreName {
			get { return "outstanding loan count"; }
		} // ScoreName
	}  // class OutstandingLoanCount
} // namespace
