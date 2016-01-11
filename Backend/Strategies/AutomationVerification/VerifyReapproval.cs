namespace Ezbob.Backend.Strategies.AutomationVerification {
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
			return new Agent(
				oRow.CustomerId,
				null,
				null,
				DB,
				Log
			).Init().MakeAndVerifyDecision(Tag);
		} // MakeAndVerifyDecision
	} // class VerifyReapproval
} // namespace
