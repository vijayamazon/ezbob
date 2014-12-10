namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OnlineOneMonthTurnover: ATrace {
		public OnlineOneMonthTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(decimal oneMonthTurnover, decimal annualTurnover, decimal dropRatio) {
			Comment = string.Format("online one month turnover {0}, annual turnover {1}, drop ratio {2}, allowed drop ration {3}",
				oneMonthTurnover, annualTurnover, annualTurnover != 0 ? (oneMonthTurnover*12 / annualTurnover).ToString("N2") : "NAN", dropRatio);
		}
	}  // class Turnover
} // namespace
