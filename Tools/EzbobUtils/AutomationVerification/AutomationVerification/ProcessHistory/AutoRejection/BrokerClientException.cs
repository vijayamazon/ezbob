namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class BrokerClientException : ABoolTrace {
		public BrokerClientException(DecisionStatus nDecisionStatus): base(nDecisionStatus){} // constructor

		protected override string PropertyName
		{
			get { return "broker client"; }
		}
	}  // class
} // namespace
