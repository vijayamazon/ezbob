namespace AutomationCalculator.ProcessHistory {
	public abstract class ANumericTrace : ATrace {
		public virtual decimal Value { get; private set; }

		public void Init(decimal nValue) {
			Value = nValue;

			Comment = string.Format("customer has {0}", ValueStr);
		} // Init

		protected ANumericTrace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected abstract string ValueStr { get; }
	} // class ANumericTrace
} // namespace
