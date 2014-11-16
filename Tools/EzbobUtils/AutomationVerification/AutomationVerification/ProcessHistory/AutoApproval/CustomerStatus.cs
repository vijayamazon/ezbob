namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class CustomerStatus : ATrace {
		public CustomerStatus(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public void Init(string sStatusName) {
			StatusName = sStatusName;
			IsEnabled = DecisionStatus == DecisionStatus.Affirmative;

			Comment = string.Format(
				"customer {0} has status '{1}' which is currently {2}abled",
				CustomerID,
				StatusName,
				IsEnabled ? "en" : "dis"
			);
		} // Init

		public string StatusName { get; private set; }
		public bool IsEnabled { get; private set; }

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<string> { StatusName });
		} // GetInitArgs
	}  // class CustomerStatus
} // namespace
