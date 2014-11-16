namespace AutomationCalculator.ProcessHistory.Common {
	public class OutstandingLoanCount : AThresholdTrace {
		public OutstandingLoanCount(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "outstanding loan count"; }
		} // ValueName
	}  // class OutstandingLoanCount
} // namespace
