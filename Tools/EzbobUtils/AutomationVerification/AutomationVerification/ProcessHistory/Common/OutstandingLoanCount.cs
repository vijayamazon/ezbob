namespace AutomationCalculator.ProcessHistory.Common {
	public class OutstandingLoanCount : AThresholdTrace {
		public OutstandingLoanCount(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "outstanding loan count"; }
		} // ValueName
	}  // class OutstandingLoanCount
} // namespace
