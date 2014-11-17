namespace AutomationCalculator.ProcessHistory {
	public abstract class ANumericTrace : ATrace {
		public virtual decimal Value { get; private set; }

		#region method Init

		public void Init(decimal nValue) {
			Value = nValue;

			Comment = string.Format("customer {0} has {1}", CustomerID, ValueStr);
		} // Init

		#endregion method Init

		#region constructor

		protected ANumericTrace(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		#endregion constructor

		protected abstract string ValueStr { get; }
	} // class ANumericTrace
} // namespace
