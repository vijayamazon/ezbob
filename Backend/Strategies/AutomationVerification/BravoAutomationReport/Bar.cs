namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData;

	public class Bar : AStrategy {
		public Bar(DateTime? startTime, DateTime? endTime) {
			this.loadAndCategorize = new BarLoadAndCategorizeDecisions(startTime, endTime);
			this.digIntoManualNoSignatureEnoughData = new Digger();
		} // constructor

		public override string Name {
			get { return "Bravo automation report"; }
		} // Name

		public override void Execute() {
			this.loadAndCategorize.Execute();
			this.digIntoManualNoSignatureEnoughData.Execute();
		} // Execute

		private readonly BarLoadAndCategorizeDecisions loadAndCategorize;
		private readonly Digger digIntoManualNoSignatureEnoughData;
	} // class Bar
} // namespace
