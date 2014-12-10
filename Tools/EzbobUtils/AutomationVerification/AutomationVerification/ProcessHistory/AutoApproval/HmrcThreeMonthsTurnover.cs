namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class HmrcThreeMonthsTurnover : ATrace {
		public HmrcThreeMonthsTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(decimal threeMonthTurnover, decimal annualTurnover, decimal dropRatio) {
			Comment = string.Format("HMRC three months turnover {0}, annual turnover {1}, drop ratio {2}, allowed drop ration {3}",
				threeMonthTurnover, annualTurnover, annualTurnover != 0 ? (threeMonthTurnover * 4 / annualTurnover).ToString("N2") : "NAN", dropRatio);
		} //Init
	}  // class HmrcThreeMonthsTurnover
} // namespace
