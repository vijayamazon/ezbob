namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class ThreeMonthsTurnover : ATrace {
		public ThreeMonthsTurnover(DecisionStatus nDecisionStatus)
			: base(nDecisionStatus) {
		} // constructor

		public void Init(decimal threeMonthTurnover, decimal annualTurnover, decimal dropRatio) {
			Comment = string.Format(
				"Three months turnover {0}, annual turnover {1}, drop ratio {2}, allowed drop ratio {3}",
				threeMonthTurnover,
				annualTurnover,
				annualTurnover != 0 ? (threeMonthTurnover * 4 / annualTurnover).ToString("N2") : "NAN",
				dropRatio
				);
		} //Init
	}  // class ThreeMonthsTurnover
} // namespace
