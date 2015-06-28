namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.LoadAndCategorize {
	using System;

	using BarCustomerDecisions =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize.CustomerDecisions;

	using BaseApproveAgent = AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent;
	using ApproveAgent = Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.AutoApprove.Agent;

	public class CustomerDecisions : BarCustomerDecisions {
		public CustomerDecisions(int customerID, bool isAlibaba, string tag) : base(customerID, isAlibaba, tag) {
		} // constructor

		protected override BaseApproveAgent CreateAutoApproveAgent(
			int offeredCreditLine,
			AutomationCalculator.Common.MedalOutputModel medal,
			DateTime decisionTime
		) {
			return new ApproveAgent(
				CustomerID,
				offeredCreditLine,
				medal.Medal,
				medal.MedalType,
				medal.TurnoverType,
				decisionTime,
				DB,
				Log
			);
		} // CreateAutoApproveAgent
	} // class CustomerDecisions
} // namespace
