namespace AutomationCalculator.ProcessHistory.Common {
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using Newtonsoft.Json;

	public class FraudSuspect : ATrace {
		#region constructor

		public FraudSuspect(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		#endregion constructor

		public void Init(FraudStatus nFraudStatus) {
			FraudStatus = nFraudStatus;

			Comment = string.Format("customer {0} fraud status is {1}", CustomerID, FraudStatus);
		} // Init

		public FraudStatus FraudStatus { get; private set; }

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<string> { FraudStatus.ToString() });
		} // GetInitArgs
	} // class FraudSuspect
} // namespace
