namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingRepayRatio : AThresholdTrace {
		public OutstandingRepayRatio(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
			Accuracy = 2;
		} // constructor

		protected override string ValueName {
			get { return "outstanding principal repay ratio"; }
		} // ValueName
	}  // class OutstandingRepayRatio
} // namespace
