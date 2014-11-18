namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class CustomerStatus : ATrace {
		public CustomerStatus(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(string sStatusName) {
			StatusName = sStatusName;
			IsEnabled = DecisionStatus == DecisionStatus.Affirmative;

			Comment = string.Format(
				"customer status '{0}' which is currently {1}abled",
				StatusName,
				IsEnabled ? "en" : "dis"
			);
		} // Init

		public string StatusName { get; private set; }
		public bool IsEnabled { get; private set; }
	}  // class CustomerStatus
} // namespace
