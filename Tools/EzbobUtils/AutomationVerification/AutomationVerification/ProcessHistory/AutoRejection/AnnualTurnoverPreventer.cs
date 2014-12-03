namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class AnnualTurnoverPreventer : AThresholdTrace {
		public AnnualTurnoverPreventer(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "Annual turnover exception"; }
		} // ValueName
	}  // class
} // namespace
