namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class ConsumerScore : AThresholdTrace {
		public ConsumerScore(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "consumer score"; }
		} // ValueName
	}  // class ConsumerScore
} // namespace
