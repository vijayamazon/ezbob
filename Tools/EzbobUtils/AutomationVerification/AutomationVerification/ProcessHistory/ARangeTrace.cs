namespace AutomationCalculator.ProcessHistory {
	public abstract class ARangeTrace : ATrace {
		public virtual ATrace Init(decimal nValue, decimal nMin, decimal nMax) {
			Value = nValue;
			Min = nMin;
			Max = nMax;

			Comment = string.Format(
				"{0} is {1}, allowed range is {2}...{3} (inclusive)",
				ValueName, Value, Min, Max
			);

			return this;
		} // Init

		public virtual decimal Value { get; private set; }
		public virtual decimal Min { get; private set; }
		public virtual decimal Max { get; private set; }

		protected ARangeTrace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected abstract string ValueName { get; }
	}  // class ARangeTrace
} // namespace
