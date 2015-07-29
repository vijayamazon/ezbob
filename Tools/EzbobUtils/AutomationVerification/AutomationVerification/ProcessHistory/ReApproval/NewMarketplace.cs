namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class NewMarketplace : ATrace {
		public NewMarketplace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(bool newMpWasAdded) {
			Comment = newMpWasAdded
						  ? "customer has added a new marketplace"
				          : "customer has not added marketplaces after the last manually approved cash request";
		}

		public void Init(string newMps) {
			if (string.IsNullOrEmpty(newMps)) {
				Comment = "customer has not added marketplaces after the last manually approved cash request";
			}else {
				Comment = "customer has added " + newMps;
			}// if
		} // Init
	} // class NewMarketplace
} // namespace
