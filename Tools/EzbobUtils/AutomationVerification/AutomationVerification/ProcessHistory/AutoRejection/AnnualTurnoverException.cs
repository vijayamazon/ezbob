namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class AnnualTurnoverException : AThresholdTrace {
		public AnnualTurnoverException(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "Annual turnover exception"; }
		} // ValueName
	}  // class
} // namespace
