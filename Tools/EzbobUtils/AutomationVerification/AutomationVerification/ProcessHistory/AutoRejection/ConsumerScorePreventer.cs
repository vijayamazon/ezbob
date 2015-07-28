namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class ConsumerScorePreventer : AThresholdTrace {
		public ConsumerScorePreventer(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "consumer score"; }
		} // ValueName
	}  // class
} // namespace
