namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	internal class AbnormalShutdownBeforeHavingCashRequest : AOneExitStep {
		public AbnormalShutdownBeforeHavingCashRequest(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		public override string Outcome { get { return "'completed'"; } }

		protected override void ExecuteStep() {
			Log.Warn("Abnormal shutdown before having cash request for {0}.", OuterContextDescription);
		} // ExecuteStep
	} // class AbnormalShutdownBeforeHavingCashRequest
} // namespace
