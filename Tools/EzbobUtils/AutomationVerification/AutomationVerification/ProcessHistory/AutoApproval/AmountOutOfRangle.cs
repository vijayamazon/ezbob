namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class AmountOutOfRangle : ARangeTrace {
		public AmountOutOfRangle(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "calculated amount"; }
		} // ValueName
	}  // class AmountOutOfRangle
} // namespace
