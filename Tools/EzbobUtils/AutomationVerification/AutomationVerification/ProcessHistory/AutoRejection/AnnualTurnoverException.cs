namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class AnnualTurnoverException : AThresholdTrace {
		public AnnualTurnoverException(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "Annual turnover exception"; }
		} // ValueName
	}  // class
} // namespace
