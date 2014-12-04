namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OnlineThreeMonthsTurnover : AThresholdTrace {
		public OnlineThreeMonthsTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "online three months turnover"; }
		} // ValueName
	}  // class OnlineThreeMonthsTurnover
} // namespace
