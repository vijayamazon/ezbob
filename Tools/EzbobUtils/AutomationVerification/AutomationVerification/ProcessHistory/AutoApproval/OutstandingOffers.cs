namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingOffers : AThresholdTrace {
		public OutstandingOffers(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "outstanding offers"; }
		} // ValueName
	}  // class OutstandingOffers
} // namespace
