namespace AutomationCalculator.ProcessHistory {
	public abstract class ARangeTrace : ATrace {
		public virtual ATrace Init(decimal nValue, decimal nMin, decimal nMax, bool isInclusive = true) {
			Value = nValue;
			Min = nMin;
			Max = nMax;

			Comment = string.Format(
				"{0} is {1}, allowed range is {2}...{3} ({4} inclusive)",
				ValueName, Value, Min, Max, isInclusive ? "" : "not"
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
