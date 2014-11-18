namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class BusinessScoreException : AThresholdTrace {
		public BusinessScoreException(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "business score exception"; }
		} // ValueName
	}  // class
} // namespace
