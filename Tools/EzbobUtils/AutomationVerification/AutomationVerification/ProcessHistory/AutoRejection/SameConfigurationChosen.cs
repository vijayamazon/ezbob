namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class SameConfigurationChosen : ATrace {
		public SameConfigurationChosen(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(int primary, int secondary) {
			Comment = string.Format(
				"Both implementations {0}agree on chosen offer configuration: " +
				"GraderRangeID = {1} in primary, GradeRangeID = {2} in verification.",
				(primary == secondary) ? string.Empty : "dis",
				primary,
				secondary
			);
		} // Init
	} // class SameConfigurationChosen
} // namespace
