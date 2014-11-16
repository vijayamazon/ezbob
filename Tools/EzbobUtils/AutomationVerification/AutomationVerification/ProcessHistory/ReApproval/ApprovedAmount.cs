namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class ApprovedAmount : ANumericTrace {
		#region constructor

		public ApprovedAmount(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		#endregion constructor

		protected override string ValueStr {
			get { return string.Format("approved amount of {0}", Value.ToString("N2")); }
		} // ValueStr
	} // class ApprovedAmount
} // namespace
