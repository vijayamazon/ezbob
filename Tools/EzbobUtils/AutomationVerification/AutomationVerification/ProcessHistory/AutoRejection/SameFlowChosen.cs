namespace AutomationCalculator.ProcessHistory.AutoRejection {
	using AutomationCalculator.Common;

	public class SameFlowChosen : ATrace {
		public SameFlowChosen(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(AutoDecisionFlowTypes primary, AutoDecisionFlowTypes secondary) {
			Comment = string.Format(
				"Both implementations {0}agree on chosen flow: " +
				"{1} flow in primary, {2} flow in verification.",
				(primary == secondary) ? string.Empty : "dis",
				primary,
				secondary
			);
		} // Init
	} // class SameFlowChosen
} // namespace
