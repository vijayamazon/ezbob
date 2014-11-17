namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OneYearTurnover : AThresholdTrace {
		public OneYearTurnover(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "one year turnover"; }
		} // ValueName
	}  // class Turnover
} // namespace
