namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingOffers : BusinessScore {
		public OutstandingOffers(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ScoreName {
			get { return "outstanding offers"; }
		} // ScoreName
	}  // class OutstandingOffers
} // namespace
