namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class HalfYearTurnover : ATrace {
		public HalfYearTurnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(decimal sixMonthTurnover, decimal annualTurnover, decimal dropRatio) {
			Comment = string.Format("HMRC six months turnover {0}, annual turnover {1}, drop ratio {2}, allowed drop ration {3}",
				sixMonthTurnover, annualTurnover, annualTurnover != 0 ? (sixMonthTurnover * 2 / annualTurnover).ToString("N2") : "NAN", dropRatio);
		} //Init
	}  // class HalfYearTurnover
} // namespace
