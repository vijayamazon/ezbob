namespace AutomationCalculator.ProcessHistory.AutoReRejection
{
	public class LRDIsTooOld : AThresholdTrace {
		public LRDIsTooOld(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		protected override string ValueName {
			get { return "last reject date"; }
		}
	}  // class 
} // namespace
