namespace AutomationCalculator.ProcessHistory {
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public abstract class AThresholdTrace : ATrace {
		protected AThresholdTrace(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public virtual ATrace Init(decimal nScore, decimal nThreshold) {
			Value = nScore;
			Threshold = nThreshold;

			Comment = string.Format(
				"customer {0} {3} is {1}, threshold is {2}",
				CustomerID,
				Value,
				Threshold,
				ValueName
			);

			return this;
		} // Init

		public virtual decimal Value { get; private set; }
		public virtual decimal Threshold { get; private set; }

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<decimal> { Value, Threshold });
		} // GetInitArgs

		protected abstract string ValueName { get; } // ValueName
	}  // class AThresholdTrace
} // namespace
