namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class TodayLoans : AThresholdTrace {
		public TodayLoans(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "sum of today loans"; }
		} // ValueName
	}  // class TodayLoans
} // namespace
