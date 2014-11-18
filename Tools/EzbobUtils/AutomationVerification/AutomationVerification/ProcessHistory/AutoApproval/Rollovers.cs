namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Rollovers : ABoolTrace {
		public Rollovers(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string PropertyName {
			get { return "rollovers"; }
		} // PropertyName
	}  // class Rollovers
} // namespace
