namespace AutomationCalculator.ProcessHistory {
	public abstract class ABoolTrace : ATrace {
		public virtual ATrace Init() {
			HasProperty = !CompletedSuccessfully;

			Comment = string.Format("customer {0} has {1}{2}", CustomerID, HasProperty ? string.Empty : "no ", PropertyName);

			return this;
		} // Init

		public virtual bool HasProperty { get; private set; }

		protected ABoolTrace(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected abstract string PropertyName { get; }
	}  // class ABoolTrace
} // namespace
