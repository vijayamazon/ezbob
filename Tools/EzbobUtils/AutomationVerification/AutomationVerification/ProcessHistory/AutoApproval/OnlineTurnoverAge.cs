namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OnlineTurnoverAge : ATurnoverAge {
		public OnlineTurnoverAge(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string TurnoverName { get { return "online"; } } // TurnoverName
	} // class ATurnoverAge
} // namespace
