namespace AutomationCalculator.ProcessHistory.Common {
	public class SameAmount : ANumericTrace {

		public SameAmount(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueStr {
			get { return string.Format("approved amount of {0}", Value); }
		} // ValueStr
	} // class SameAmount
} // namespace
