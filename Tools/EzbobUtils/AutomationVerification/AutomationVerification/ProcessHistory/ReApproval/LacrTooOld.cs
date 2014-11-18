namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class LacrTooOld : AThresholdTrace {
		public LacrTooOld(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "last manually approved cash request age in days"; }
		} // ValueName
	} // class LacrTooOld
} // namespace
