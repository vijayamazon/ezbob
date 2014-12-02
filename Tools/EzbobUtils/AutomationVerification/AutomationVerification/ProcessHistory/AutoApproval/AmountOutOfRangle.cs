namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class AmountOutOfRangle : ARangeTrace {
		public AmountOutOfRangle(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(int amount, bool isInSilentMode) {
			if (isInSilentMode) {
				Comment = string.Format("Amount: {0} not checking for range in silent mode", amount);
			}
		}
		protected override string ValueName {
			get { return "calculated amount"; }
		} // ValueName
	}  // class AmountOutOfRangle
} // namespace
