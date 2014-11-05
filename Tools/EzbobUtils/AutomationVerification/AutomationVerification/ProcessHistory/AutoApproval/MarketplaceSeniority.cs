namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class MarketplaceSeniority : BusinessScore {
		public MarketplaceSeniority(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ScoreName {
			get { return "marketplace seniority"; }
		} // ScoreName
	}  // class MarketplaceSeniority
} // namespace
