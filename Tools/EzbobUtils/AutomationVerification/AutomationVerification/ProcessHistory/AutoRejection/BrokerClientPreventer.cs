namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class BrokerClientPreventer : ATrace {
		public BrokerClientPreventer(DecisionStatus nDecisionStatus): base(nDecisionStatus){} // constructor

		public void Init(bool isBrokerClient) {
			Comment = string.Format("Customer is {0}broker's client", isBrokerClient ? "" : "not ");
		}
	}  // class
} // namespace
