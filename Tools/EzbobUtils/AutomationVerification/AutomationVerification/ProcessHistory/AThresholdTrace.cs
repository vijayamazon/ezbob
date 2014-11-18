namespace AutomationCalculator.ProcessHistory {
	public abstract class AThresholdTrace : ATrace {
		protected AThresholdTrace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public virtual ATrace Init(decimal nScore, decimal nThreshold) {
			Value = nScore;
			Threshold = nThreshold;

			Comment = string.Format(
				"{0} is {1}, threshold is {2}",
				ValueName,
				Value,
				Threshold
			);

			return this;
		} // Init

		public virtual decimal Value { get; private set; }
		public virtual decimal Threshold { get; private set; }

		protected abstract string ValueName { get; } // ValueName
	}  // class AThresholdTrace
} // namespace
