namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class AmlCheck : ATrace {
		public AmlCheck(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public string AmlResult { get; private set; }

		public void Init(string sAmlResult) {
			AmlResult = sAmlResult;

			Comment = string.Format("AML result is '{0}'", AmlResult);
		} // Init
	} // class AmlCheck
} // namespace