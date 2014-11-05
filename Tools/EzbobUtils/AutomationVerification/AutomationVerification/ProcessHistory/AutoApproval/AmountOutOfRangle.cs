namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class AmountOutOfRangle : ATrace {
		public AmountOutOfRangle(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public virtual ATrace Init(int nValue, int nMin, int nMax) {
			Value = nValue;
			Min = nMin;
			Max = nMax;

			Comment = string.Format(
				"customer {0} {3} is {1}, allowed range is {2}...{3} (inclusive)",
				CustomerID, Value, Min, Max, ValueName
			);

			return this;
		} // Init

		public int Value { get; private set; }
		public int Min { get; private set; }
		public int Max { get; private set; }

		protected virtual string ValueName {
			get { return "calculated amount"; }
		} // ValueName
	}  // class AmountOutOfRangle
} // namespace
