namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class BrokerClientPreventer : ABoolTrace {
		public BrokerClientPreventer(DecisionStatus nDecisionStatus): base(nDecisionStatus){} // constructor

		protected override string PropertyName
		{
			get { return "broker client"; }
		}
	}  // class
} // namespace
