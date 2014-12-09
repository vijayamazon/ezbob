namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class ApprovedAmount : ANumericTrace {

		public ApprovedAmount(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueStr {
			get { return string.Format("approved amount of {0}", Value.ToString("N2")); }
		} // ValueStr
	} // class ApprovedAmount
} // namespace
