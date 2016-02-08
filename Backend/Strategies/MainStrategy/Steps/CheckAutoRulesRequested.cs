namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Models;

	internal class CheckAutoRulesRequested : AMainStrategyStep {
		public CheckAutoRulesRequested(
			string outerContextDescription,
			NewCreditLineOption newCreditLineOption
		) : base(outerContextDescription) {
			this.newCreditLineOption = newCreditLineOption;
		} // constructor

		public override string Outcome {
			get {
				return this.newCreditLineOption.AvoidAutoDecision()
					? "'not requested auto rules'"
					: "'requested auto rules'";
			} // get
		} // Outcome

		protected override StepResults Run() {
			return this.newCreditLineOption.AvoidAutoDecision()
				? StepResults.NotRequested
				: StepResults.Requested;
		} // Run

		private readonly NewCreditLineOption newCreditLineOption;
	} // class CheckAutoRulesRequested
} // namespace
