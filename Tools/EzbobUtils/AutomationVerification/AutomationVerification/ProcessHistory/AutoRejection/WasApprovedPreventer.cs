namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class WasApprovedPreventer : ATrace {
		public WasApprovedPreventer(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		public void Init(bool wasApproved) {
			Comment = string.Format("Customer {0} approved for a loan", wasApproved ? "was" : "wasn't");
		}
	}  // class 
} // namespace
