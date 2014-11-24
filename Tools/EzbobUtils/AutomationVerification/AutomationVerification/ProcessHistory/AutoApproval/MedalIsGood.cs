namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using AutomationCalculator.Common;

	public class MedalIsGood : ATrace {
		public MedalIsGood(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public Medal MedalName { get; private set; }

		public void Init(Medal sMedalName) {
			MedalName = sMedalName;

			Comment = string.Format("Medal name is '{0}'", MedalName);
		} // Init
	} // class MedalIsGood
} // namespace