namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class DefaultAccounts : ATrace {
		public DefaultAccounts(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public virtual ATrace Init() {
			HasProperty = !CompletedSuccessfully;

			Comment = string.Format("customer {0} has {1}{2}", CustomerID, HasProperty ? string.Empty : "no ", PropertyName);

			return this;
		} // Init

		public virtual bool HasProperty { get; private set; }

		protected virtual string PropertyName {
			get { return "default accounts"; }
		} // PropertyName
	}  // class DefaultAccounts
} // namespace
