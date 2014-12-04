namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class HmrcTurnoverAge : ATurnoverAge {
		public HmrcTurnoverAge(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string TurnoverName { get { return "HMRC"; } } // TurnoverName
	} // class ATurnoverAge
} // namespace
