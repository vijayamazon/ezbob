namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class BrokerClientException : ABoolTrace {
		public BrokerClientException(int nCustomerID, DecisionStatus nDecisionStatus): base(nCustomerID, nDecisionStatus){} // constructor

		protected override string PropertyName
		{
			get { return "broker client"; }
		}
	}  // class
} // namespace
