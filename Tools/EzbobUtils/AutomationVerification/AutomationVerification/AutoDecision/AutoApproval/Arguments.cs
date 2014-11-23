namespace AutomationCalculator.AutoDecision.AutoApproval {
	/// <summary>
	/// Contains input arguments for auto approval logic.
	/// </summary>
	public class Arguments {
		#region constructor

		public Arguments(int nCustomerID, decimal nSystemCalculatedAmount) {
			CustomerID = nCustomerID;
			SystemCalculatedAmount = nSystemCalculatedAmount;
		} // constructor

		#endregion constructor

		public int CustomerID { get; private set; }
		public decimal SystemCalculatedAmount { get; private set; }
	} // Arguments
} // namespace
