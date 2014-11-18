namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class WasApproved : ABoolTrace {
		public WasApproved(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {} // constructor

		protected override string PropertyName {
			get { return "Was approved for a loan"; }
		} // PropertyName
	}  // class 
} // namespace
