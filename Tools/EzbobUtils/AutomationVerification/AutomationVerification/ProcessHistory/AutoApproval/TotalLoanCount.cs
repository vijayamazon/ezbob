namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using Ezbob.Utils.Lingvo;

	public class TotalLoanCount : ANumericTrace {
		#region constructor

		public TotalLoanCount(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		#endregion constructor

		protected override string ValueStr {
			get { return Grammar.Number((int)Value, "loan"); }
		} // ValueStr
	} // class TotalLoanCount
} // namespace
