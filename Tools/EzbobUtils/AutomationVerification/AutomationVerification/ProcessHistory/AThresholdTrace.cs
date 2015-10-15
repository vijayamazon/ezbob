namespace AutomationCalculator.ProcessHistory {
	public abstract class AThresholdTrace : ATrace {
		protected AThresholdTrace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
			Accuracy = 0;
		} // constructor

		public virtual ATrace Init(decimal value, decimal threshold, bool isPlural = false, string units = "") {
			Value = value;
			Threshold = threshold;

			string numFormat = "N" + Accuracy;

			Comment = string.Format(
				"{0} {3} {1} {4}, threshold is {2} {4}",
				ValueName,
				Value.ToString(numFormat),
				Threshold.ToString(numFormat),
				isPlural ? "are" : "is",
				units
			);

			return this;
		} // Init

		public virtual decimal Value { get; private set; }
		public virtual decimal Threshold { get; private set; }

		protected abstract string ValueName { get; } // ValueName

		protected int Accuracy { get; set; }
	}  // class AThresholdTrace
} // namespace
