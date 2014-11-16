namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingOffers : AThresholdTrace {
		public OutstandingOffers(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "outstanding offers"; }
		} // ValueName
	}  // class OutstandingOffers
} // namespace
