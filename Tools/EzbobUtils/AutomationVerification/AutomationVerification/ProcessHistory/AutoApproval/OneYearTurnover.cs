namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OneYearTurnover : AThresholdTrace {
		public OneYearTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "one year turnover"; }
		} // ValueName
	}  // class Turnover
} // namespace
