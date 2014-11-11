namespace AutomationCalculator.ProcessHistory {
	public abstract class ARangeTrace : ATrace {
		public virtual ATrace Init(decimal nValue, decimal nMin, decimal nMax) {
			Value = nValue;
			Min = nMin;
			Max = nMax;

			Comment = string.Format(
				"customer {0} {4} is {1}, allowed range is {2}...{3} (inclusive)",
				CustomerID, Value, Min, Max, ValueName
			);

			return this;
		} // Init

		public virtual decimal Value { get; private set; }
		public virtual decimal Min { get; private set; }
		public virtual decimal Max { get; private set; }

		protected ARangeTrace(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected abstract string ValueName { get; }
	}  // class ARangeTrace
} // namespace
