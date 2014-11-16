namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class HomeOwnerCap : AThresholdTrace {
		public HomeOwnerCap(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "approved amount for cap check"; }
		} // ValueName
	}  // class HomeOwnerCap
} // namespace
