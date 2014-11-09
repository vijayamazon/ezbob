namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class MarketplaceSeniority : AThresholdTrace {
		public MarketplaceSeniority(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "marketplace seniority"; }
		} // ValueName
	}  // class MarketplaceSeniority
} // namespace
