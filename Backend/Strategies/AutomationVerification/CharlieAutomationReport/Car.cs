namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport {
	using System;
	using Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport;

	using BarLoadAndCategorizeDecisions =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize.LoadAndCategorizeDecisions;

	using CarLoadAndCategorizeDecisions =
		Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.LoadAndCategorize.LoadAndCategorizeDecisions;

	public class Car : Bar {
		public Car(DateTime? startTime, DateTime? endTime) : base(startTime, endTime) {
		} // constructor

		public override string Name {
			get { return "Charlie automation report"; }
		} // Name

		protected override BarLoadAndCategorizeDecisions CreateLoadAndCategorize() {
			return new CarLoadAndCategorizeDecisions(StartTime, EndTime);
		} // LoadAndCategorize
	} // class Car
} // namespace
