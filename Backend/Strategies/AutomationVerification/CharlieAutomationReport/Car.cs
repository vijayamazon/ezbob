namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport {
	using System;
	using Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport;

	using BarLoadAndCategorizeDecisions =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize.LoadAndCategorizeDecisions;

	using CarLoadAndCategorizeDecisions =
		Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.LoadAndCategorize.LoadAndCategorizeDecisions;

	using BaseDigger =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData.Digger;

	using CarDigger =
		Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.DigIntoManualNoSignatureEnoughData.Digger;

	public class Car : Bar {
		public Car(DateTime? startTime, DateTime? endTime) : base(startTime, endTime) {
		} // constructor

		public override string Name {
			get { return "Charlie automation report"; }
		} // Name

		public override void Execute() {
			// SkipLoadAndCategorize = true;
			base.Execute();
		} // Execute

		protected override BarLoadAndCategorizeDecisions CreateLoadAndCategorize() {
			return new CarLoadAndCategorizeDecisions(StartTime, EndTime);
		} // LoadAndCategorize

		protected override BaseDigger CreateDigger() {
			return new CarDigger();
		} // CreateDigger
	} // class Car
} // namespace
