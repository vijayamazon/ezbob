namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class LateLoans : ABoolTrace {
		public LateLoans(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string PropertyName {
			get { return "late loans"; }
		} // PropertyName
	} // class WorstCaisStatus
} // namespace