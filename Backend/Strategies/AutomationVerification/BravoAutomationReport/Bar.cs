namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData;
	using Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize;

	public class Bar : AStrategy {
		public Bar(DateTime? startTime, DateTime? endTime) {
			this.skipLoadAndCategorize = false;
			StartTime = startTime;
			EndTime = endTime;
		} // constructor

		public override string Name {
			get { return "Bravo automation report"; }
		} // Name

		public override void Execute() {
			if (!SkipLoadAndCategorize)
				CreateLoadAndCategorize().Execute();

			CreateDigger().Execute();
		} // Execute

		protected virtual DateTime? StartTime { get; private set; }
		protected virtual DateTime? EndTime { get; private set; }

		protected virtual LoadAndCategorizeDecisions CreateLoadAndCategorize() {
			return new LoadAndCategorizeDecisions(StartTime, EndTime);
		} // LoadAndCategorize

		protected virtual Digger CreateDigger() {
			return new Digger();
		} // CreateDigger

		protected virtual bool SkipLoadAndCategorize {
			get { return this.skipLoadAndCategorize; }
			set { this.skipLoadAndCategorize = value; }
		} // SkipLoadAndCategorize

		private bool skipLoadAndCategorize;
	} // class Bar
} // namespace
