namespace AutomationCalculator.ProcessHistory.AutoReRejection
{
	public class OpenLoans : AThresholdTrace {
		public OpenLoans(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		protected override string ValueName {
			get { return "number of open loans"; }
		}
	}  // class 
} // namespace
