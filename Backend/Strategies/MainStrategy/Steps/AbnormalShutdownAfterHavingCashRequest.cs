namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	internal class AbnormalShutdownAfterHavingCashRequest : AOneExitStep {
		public AbnormalShutdownAfterHavingCashRequest(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		protected override void ExecuteStep() {
			Log.Warn("Abnormal shutdown after having cash request for {0}.", OuterContextDescription);
		} // ExecuteStep
	} // class AbnormalShutdownAfterHavingCashRequest
} // namespace
