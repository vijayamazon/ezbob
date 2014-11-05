namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Complete : ATrace {
		public Complete(int nCustomerID, bool bCompletedSuccessfully)
			: base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public void Init(int nApprovedAmount) {
			ApprovedAmount = nApprovedAmount;

			Comment = string.Format("customer {0} approved amount is {1}", CustomerID, ApprovedAmount);
		} // Init

		public int ApprovedAmount { get; private set; }
	}  // class Complete
} // namespace
