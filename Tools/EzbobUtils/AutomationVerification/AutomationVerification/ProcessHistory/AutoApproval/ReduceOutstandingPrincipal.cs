namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class ReduceOutstandingPrincipal : ATrace {
		public ReduceOutstandingPrincipal(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public void Init(decimal nOutstandingPrincipal, decimal nNewAutoApprovedAmount) {
			OutstandingPrincipal = nOutstandingPrincipal;
			NewAutoApprovedAmount = nNewAutoApprovedAmount;

			Comment = string.Format(
				"customer {0}: after reducing outstanding principal of {1} approved amount is {2}",
				CustomerID,
				OutstandingPrincipal,
				NewAutoApprovedAmount
			);
		} // Init

		public decimal OutstandingPrincipal { get; private set; }
		public decimal NewAutoApprovedAmount { get; private set; }

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<decimal> { OutstandingPrincipal, NewAutoApprovedAmount });
		} // GetInitArgs
	}  // class ReduceOutstandingPrincipal
} // namespace
