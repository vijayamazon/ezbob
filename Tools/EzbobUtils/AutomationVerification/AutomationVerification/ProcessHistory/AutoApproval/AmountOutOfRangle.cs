namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class AmountOutOfRangle : ARangeTrace {
		public AmountOutOfRangle(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "calculated amount"; }
		} // ValueName
	}  // class AmountOutOfRangle
} // namespace
