namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Rollovers : ABoolTrace {
		public Rollovers(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected override string PropertyName {
			get { return "rollovers"; }
		} // PropertyName
	}  // class Rollovers
} // namespace
