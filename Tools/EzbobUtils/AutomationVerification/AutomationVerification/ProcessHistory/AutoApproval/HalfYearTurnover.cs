namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class HalfYearTurnover : AThresholdTrace {
		public HalfYearTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "six months turnover"; }
		} // ValueName
	}  // class HalfYearTurnover
} // namespace
