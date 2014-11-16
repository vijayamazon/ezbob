namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Turnover : AThresholdTrace {
		public Turnover(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public virtual string PeriodName { get; set; }

		protected override string ValueName {
			get { return PeriodName + " turnover"; }
		} // ValueName
	}  // class Turnover
} // namespace
