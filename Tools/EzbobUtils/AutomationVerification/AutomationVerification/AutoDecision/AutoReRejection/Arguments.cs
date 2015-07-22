namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;

	/// <summary>
	/// Contains input arguments for auto approval logic.
	/// </summary>
	public class Arguments {
		public Arguments(int nCustomerID, long? cashRequestID, DateTime? oDataAsOf) {
			CustomerID = nCustomerID;
			CashRequestID = cashRequestID;
			DataAsOf = oDataAsOf ?? DateTime.UtcNow;
		} // constructor

		public int CustomerID { get; private set; }
		public DateTime DataAsOf { get; private set; }
		public long? CashRequestID { get; private set; }
	} // Arguments
} // namespace
