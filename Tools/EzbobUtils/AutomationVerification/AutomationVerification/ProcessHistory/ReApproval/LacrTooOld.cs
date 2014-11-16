namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class LacrTooOld : AThresholdTrace {
		public LacrTooOld(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "last manually approved cash request age in days"; }
		} // ValueName
	} // class LacrTooOld
} // namespace
