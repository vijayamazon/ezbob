namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Misc;

	internal class FinishWizardIfRequested : AOneExitStep {
		public FinishWizardIfRequested(
			string outerContextDescription,
			FinishWizardArgs args
		) : base(outerContextDescription) {
			this.args = args;
		} // constructor

		protected override void ExecuteStep() {
			if (this.args != null)
				new FinishWizard(this.args).Execute();
		} // ExecuteStep

		private readonly FinishWizardArgs args;
	} // class FinishWizardIfRequested
} // namespace
