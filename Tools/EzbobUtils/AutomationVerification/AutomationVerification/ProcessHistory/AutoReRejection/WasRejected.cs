namespace AutomationCalculator.ProcessHistory.AutoReRejection
{
	public class WasRejected : ABoolTrace {
		public WasRejected(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		protected override string PropertyName {
			get { return "been manually rejected"; }
		} // PropertyName
	}  // class 
} // namespace
