namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;

	/// <summary>
	/// Contains input arguments for auto approval logic.
	/// </summary>
	public class Arguments {

		public Arguments(int nCustomerID, DateTime? oDataAsOf) {
			CustomerID = nCustomerID;
			DataAsOf = oDataAsOf ?? DateTime.UtcNow;
		} // constructor

		public int CustomerID { get; private set; }
		public DateTime DataAsOf { get; private set; }
	} // Arguments
} // namespace
