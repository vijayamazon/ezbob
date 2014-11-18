namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class ConsumerScoreException : AThresholdTrace {
		public ConsumerScoreException(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "consumer score exception"; }
		} // ValueName
	}  // class
} // namespace
