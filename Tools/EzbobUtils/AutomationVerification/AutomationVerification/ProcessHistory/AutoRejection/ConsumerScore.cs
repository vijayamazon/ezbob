namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class ConsumerScore : AThresholdTrace {
		public ConsumerScore(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "consumer score"; }
		} // ValueName
	}  // class ConsumerScore
} // namespace
