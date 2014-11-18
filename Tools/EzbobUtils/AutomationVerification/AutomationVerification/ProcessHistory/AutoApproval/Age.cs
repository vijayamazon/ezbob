namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Age : ARangeTrace {
		public Age(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "age"; }
		} // ValueName
	}  // class Age
} // namespace
