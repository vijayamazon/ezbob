namespace AutomationCalculator.ProcessHistory.Common {
	using Ezbob.Utils.Lingvo;

	public class TotalLoanCount : ANumericTrace {
		#region constructor

		public TotalLoanCount(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		#endregion constructor

		protected override string ValueStr {
			get { return Grammar.Number((int)Value, "loan"); }
		} // ValueStr
	} // class TotalLoanCount
} // namespace
