namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class ThreeMonthsTurnover : AThresholdTrace {
		public ThreeMonthsTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "three months turnover"; }
		} // ValueName
	}  // class Turnover
} // namespace
