namespace AutomationCalculator.ProcessHistory.Common {
	public class Complete : ATrace {
		public Complete(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public void Init(decimal nApprovedAmount) {
			ApprovedAmount = nApprovedAmount;

			Comment = string.Format("customer {0} approved amount is {1}", CustomerID, ApprovedAmount.ToString("N2"));
		} // Init

		public decimal ApprovedAmount { get; private set; }
	}  // class Complete
} // namespace
