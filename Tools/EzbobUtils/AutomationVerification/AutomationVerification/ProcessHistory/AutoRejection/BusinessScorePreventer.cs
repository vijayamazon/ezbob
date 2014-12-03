namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class BusinessScorePreventer : AThresholdTrace {
		public BusinessScorePreventer(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "business score exception"; }
		} // ValueName
	}  // class
} // namespace
