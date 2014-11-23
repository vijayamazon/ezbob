namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class CustomerStatus: ATrace
	{
		public CustomerStatus(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { }

		public void Init(string customerStatus) {
			Comment = string.Format("customer status is {0} (allowed Enabled, Fraud Suspect)", customerStatus);
		} // Init

	}
}
