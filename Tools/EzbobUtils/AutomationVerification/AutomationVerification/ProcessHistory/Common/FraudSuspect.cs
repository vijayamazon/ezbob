namespace AutomationCalculator.ProcessHistory.Common {
	using EZBob.DatabaseLib.Model.Database;

	public class FraudSuspect : ATrace {
		#region constructor

		public FraudSuspect(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		#endregion constructor

		public void Init(FraudStatus nFraudStatus) {
			FraudStatus = nFraudStatus;

			Comment = string.Format("customer {0} fraud status is {1}", CustomerID, FraudStatus);
		} // Init

		public FraudStatus FraudStatus { get; private set; }
	} // class FraudSuspect
} // namespace
