namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	internal class AbnormalShutdownAfterCashRequestWasWritten : AOneExitStep {
		public AbnormalShutdownAfterCashRequestWasWritten(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		public override string Outcome { get { return "'completed'"; } }

		protected override void ExecuteStep() {
			Log.Warn("Abnormal shutdown after cash request was written for {0}.", OuterContextDescription);
		} // ExecuteStep
	} // class AbnormalShutdownAfterCashRequestWasWritten
} // namespace
