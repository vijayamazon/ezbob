namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using Ezbob.Backend.Models;

	internal class CheckUpdateDataRequested : ATwoExitStep {
		public CheckUpdateDataRequested(
			string outerContextDescription,
			AMainStrategyStep onRequested,
			AMainStrategyStep onNotRequestedWithAutoRules,
			AMainStrategyStep onNotRequestedWithoutAutoRules,
			NewCreditLineOption newCreditLineOption
		) : base(outerContextDescription, onRequested, onNotRequestedWithAutoRules) {
			this.newCreditLineOption = newCreditLineOption;
			this.onNotRequestedWithoutAutoRules = onNotRequestedWithoutAutoRules;
		} // constructor

		protected override string Outcome {
			get {
				if (this.newCreditLineOption.UpdateData())
					return "'requested'";

				return this.newCreditLineOption.AvoidAutoDecision()
					? "'not requested without auto rules'"
					: "'not requested with auto rules'";
			} // get
		} // Outcome

		protected override AMainStrategyStepBase Run() {
			if (this.newCreditLineOption.UpdateData())
				return OnRequested;

			return this.newCreditLineOption.AvoidAutoDecision()
				? this.onNotRequestedWithoutAutoRules
				: OnNotRequestedWithAutoRules;
		} // Run

		private AMainStrategyStep OnRequested { get { return FirstExit; } }
		private AMainStrategyStep OnNotRequestedWithAutoRules { get { return SecondExit; } }
		private readonly AMainStrategyStep onNotRequestedWithoutAutoRules;

		private readonly NewCreditLineOption newCreditLineOption;
	} // class CheckUpdateDataRequested
} // namespace
