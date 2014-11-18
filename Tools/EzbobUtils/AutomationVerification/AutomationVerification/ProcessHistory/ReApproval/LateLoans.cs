namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class LateLoans : ABoolTrace {
		public LateLoans(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string PropertyName {
			get { return "late loans"; }
		} // PropertyName
	} // class WorstCaisStatus
} // namespace