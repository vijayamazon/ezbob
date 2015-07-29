namespace AutomationCalculator.ProcessHistory {
	public abstract class AThresholdTrace : ATrace {
		protected AThresholdTrace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public virtual ATrace Init(decimal value, decimal threshold, bool isPlural = false, string units = "") {
			Value = value;
			Threshold = threshold;

			Comment = string.Format(
				"{0} {3} {1:N0} {4}, threshold is {2:N0} {4}",
				ValueName,
				Value,
				Threshold,
				isPlural ? "are" : "is",
				units
			);

			return this;
		} // Init

		public virtual decimal Value { get; private set; }
		public virtual decimal Threshold { get; private set; }

		protected abstract string ValueName { get; } // ValueName
	}  // class AThresholdTrace
} // namespace
