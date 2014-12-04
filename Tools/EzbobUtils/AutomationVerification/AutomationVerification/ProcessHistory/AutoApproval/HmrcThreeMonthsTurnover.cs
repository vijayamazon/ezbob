namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class HmrcThreeMonthsTurnover : AThresholdTrace {
		public HmrcThreeMonthsTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "HMRC three months turnover"; }
		} // ValueName
	}  // class HmrcThreeMonthsTurnover
} // namespace
