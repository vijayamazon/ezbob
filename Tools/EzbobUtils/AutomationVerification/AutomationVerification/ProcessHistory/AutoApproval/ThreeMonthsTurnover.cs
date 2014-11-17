namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class ThreeMonthsTurnover : AThresholdTrace {
		public ThreeMonthsTurnover(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "three months turnover"; }
		} // ValueName
	}  // class Turnover
} // namespace
