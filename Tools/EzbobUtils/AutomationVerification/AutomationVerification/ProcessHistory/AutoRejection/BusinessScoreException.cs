namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class BusinessScoreException : AThresholdTrace {
		public BusinessScoreException(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "business score exception"; }
		} // ValueName
	}  // class
} // namespace
