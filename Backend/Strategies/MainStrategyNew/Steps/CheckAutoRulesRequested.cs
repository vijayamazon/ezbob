namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using Ezbob.Backend.Models;

	internal class CheckAutoRulesRequested : ATwoExitStep {
		public CheckAutoRulesRequested(
			string outerContextDescription,
			AMainStrategyStep onRequested,
			AMainStrategyStep onNotRequested,
			NewCreditLineOption newCreditLineOption
		) : base(outerContextDescription, onRequested, onNotRequested) {
			this.newCreditLineOption = newCreditLineOption;
		} // constructor

		protected override string Outcome {
			get {
				return this.newCreditLineOption.AvoidAutoDecision()
					? "'not requested auto rules'"
					: "'requested auto rules'";
			} // get
		} // Outcome

		protected override AMainStrategyStepBase Run() {
			return this.newCreditLineOption.AvoidAutoDecision()
				? OnNotRequestedWithAutoRules
				: OnRequested;
		} // Run

		private AMainStrategyStep OnRequested { get { return FirstExit; } }
		private AMainStrategyStep OnNotRequestedWithAutoRules { get { return SecondExit; } }

		private readonly NewCreditLineOption newCreditLineOption;
	} // class CheckAutoRulesRequested
} // namespace
