namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class BusinessScore : AThresholdTrace {
		public BusinessScore(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "business score"; }
		} // ValueName
	}  // class
} // namespace
