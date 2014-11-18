namespace AutomationCalculator.ProcessHistory.Common {
	public class Complete : ANumericTrace {
		public Complete(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueStr {
			get { return string.Format("approved amount is {0}", Value.ToString("N2")); }
		} // ValueStr
	}  // class Complete
} // namespace
