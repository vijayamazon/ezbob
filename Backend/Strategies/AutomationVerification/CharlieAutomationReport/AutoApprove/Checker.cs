namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.AutoApprove {
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory.AutoApproval;

	using BaseChecker = AutomationCalculator.AutoDecision.AutoApproval.Checker;
	using BaseApproveAgent = AutomationCalculator.AutoDecision.AutoApproval.Agent;
	using ApproveAgent = Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.AutoApprove.Agent;

	internal class Checker : BaseChecker {
		public Checker(BaseApproveAgent agent) : base(agent) {
		} // constructor

		public override void CaisStatuses() {
			ApproveAgent agent = Agent as ApproveAgent;

			if (agent == null) {
				base.CaisStatuses();
				return;
			} // if

			var diff = new List<string>(agent.FindBadCaisStatuses());

			if (diff.Count > 0)
				StepFailed<WorstCaisStatus>().Init(diff);
			else
				StepDone<WorstCaisStatus>().Init(null);
		} // CaisStatuses
	} // class Checker
} // namespace
