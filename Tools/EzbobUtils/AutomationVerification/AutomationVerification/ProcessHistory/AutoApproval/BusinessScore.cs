namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class BusinessScore : AThresholdTrace {
		public BusinessScore(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "business score"; }
		} // ValueName
	}  // class BusinessScore
} // namespace
