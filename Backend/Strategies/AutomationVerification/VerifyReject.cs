namespace Ezbob.Backend.Strategies.AutomationVerification {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject;

	public class VerifyReject : AVerificationBase {
		public VerifyReject(
			int nTopCount,
			int nLastCheckedCustomerID
		) : base(nTopCount, nLastCheckedCustomerID) {
		} // constructor

		protected override string DecisionName {
			get { return "Auto reject"; }
		} // DecisionName

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			return new Agent(
				oRow.CustomerId,
				DB,
				Log
			).Init().MakeAndVerifyDecision();
		} // MakeAndVerifyDecision
	} // class VerifyReject
} // namespace
