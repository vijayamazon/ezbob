namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using Ezbob.Utils.Lingvo;

	public class DefaultAccounts : ANumericTrace {
		public DefaultAccounts(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected override string ValueStr {
			get { return string.Format("{0}", Grammar.Number((int)Value, "default account")); }
		} // ValueStr
	}  // class DefaultAccounts
} // namespace
