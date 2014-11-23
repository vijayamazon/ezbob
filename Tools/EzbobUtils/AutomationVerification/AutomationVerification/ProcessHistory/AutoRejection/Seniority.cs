namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class Seniority : ARangeTrace {
		public Seniority(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		protected override string ValueName {
			get { return "seniority"; }
		} // ValueName
	}  // class
} // namespace
