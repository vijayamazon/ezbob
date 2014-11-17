namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OneMonthTurnover : AThresholdTrace {
		public OneMonthTurnover(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "one month turnover"; }
		} // ValueName
	}  // class Turnover
} // namespace
