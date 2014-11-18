namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class MarketplaceSeniority : AThresholdTrace {
		public MarketplaceSeniority(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "marketplace seniority"; }
		} // ValueName
	}  // class MarketplaceSeniority
} // namespace
