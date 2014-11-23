namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class ConsumerScore : ARangeTrace {
		public ConsumerScore(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "consumer score"; }
		} // ValueName
	}  // class ConsumerScore
} // namespace
