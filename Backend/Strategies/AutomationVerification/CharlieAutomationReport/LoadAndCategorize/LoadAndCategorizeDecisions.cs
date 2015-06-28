namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.LoadAndCategorize {
	using System;

	using BarLoadAndCategorizeDecisions =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize.LoadAndCategorizeDecisions;

	using BarCustomerDecisions =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize.CustomerDecisions;

	public class LoadAndCategorizeDecisions : BarLoadAndCategorizeDecisions {
		public LoadAndCategorizeDecisions(DateTime? startTime, DateTime? endTime) : base(startTime, endTime) {
		} // constructor

		protected override BarCustomerDecisions CreateCustomerDecisionsInstance(int customerID, bool isAlibaba) {
			return new CustomerDecisions(customerID, isAlibaba, Tag);
		} // CreateCustomerDecisionsInstance
	} // class LoadAndCategorizeDecisions
} // namespace
