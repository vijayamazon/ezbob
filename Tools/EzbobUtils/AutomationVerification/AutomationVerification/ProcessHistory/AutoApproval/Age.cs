namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Age : ARangeTrace {
		public Age(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string ValueName {
			get { return "age"; }
		} // ValueName
	}  // class Age
} // namespace
