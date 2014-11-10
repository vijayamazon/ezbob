namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class ApprovedAmount : ATrace {
		#region constructor

		public ApprovedAmount(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		#endregion constructor

		public void Init(decimal nApprovedAmount) {
			Amount = nApprovedAmount;

			Comment = string.Format("customer {0} approved amount is {1}", CustomerID, Amount.ToString("N2"));
		} // Init

		public decimal Amount { get; private set; }
	} // class ApprovedAmount
} // namespace
