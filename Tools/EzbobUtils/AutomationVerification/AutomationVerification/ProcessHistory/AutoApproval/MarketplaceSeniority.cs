namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class MarketplaceSeniority : AThresholdTrace {
		public MarketplaceSeniority(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "marketplace seniority"; }
		} // ValueName
	}  // class MarketplaceSeniority
} // namespace
