﻿namespace Ezbob.Backend.Strategies.AutomationVerification {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval;

	public class VerifyReapproval : AVerificationBase {
		public VerifyReapproval(
			int nTopCount,
			int nLastCheckedCustomerID
		) : base(nTopCount, nLastCheckedCustomerID) {
		} // constructor

		protected override string DecisionName {
			get { return "Auto Re-approval"; }
		} // DecisionName

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			var agent = new Agent(oRow.CustomerId, null, null, DB, Log);

			agent.Init().MakeAndVerifyDecision(Tag);

			return !agent.ExceptionWhileDeciding && !agent.WasMismatch;
		} // MakeAndVerifyDecision
	} // class VerifyReapproval
} // namespace
