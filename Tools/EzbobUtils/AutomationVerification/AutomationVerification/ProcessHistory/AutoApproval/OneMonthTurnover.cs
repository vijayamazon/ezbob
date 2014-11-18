namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OneMonthTurnover : AThresholdTrace {
		public OneMonthTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "one month turnover"; }
		} // ValueName
	}  // class Turnover
} // namespace
