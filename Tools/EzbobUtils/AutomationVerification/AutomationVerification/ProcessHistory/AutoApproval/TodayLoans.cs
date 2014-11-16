namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class TodayLoans : AThresholdTrace {
		public TodayLoans(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "today loans"; }
		} // ValueName
	}  // class TodayLoans
} // namespace
