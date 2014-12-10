namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OnlineThreeMonthsTurnover : ATrace {
		public OnlineThreeMonthsTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(decimal threeMonthTurnover, decimal annualTurnover, decimal dropRatio) {
			Comment = string.Format("online three months turnover {0}, annual turnover {1}, drop ratio {2}, allowed drop ration {3}",
				threeMonthTurnover, annualTurnover, annualTurnover != 0 ? (threeMonthTurnover * 4 / annualTurnover).ToString("N2") : "NAN", dropRatio);
		}
	}  // class OnlineThreeMonthsTurnover
} // namespace
