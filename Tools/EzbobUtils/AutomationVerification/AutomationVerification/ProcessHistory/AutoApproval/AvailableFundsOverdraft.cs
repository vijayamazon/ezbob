namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class AvailableFundsOverdraft : AThresholdTrace {
		public AvailableFundsOverdraft(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "Available funds - approved amount"; }
		} // ValueName
	}  // class AvailableFundsOverdraft
} // namespace
