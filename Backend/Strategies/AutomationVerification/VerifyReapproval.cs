namespace Ezbob.Backend.Strategies.AutomationVerification {
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
			return new Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval.Agent(
				oRow.CustomerId,
				DB,
				Log
			).Init().MakeAndVerifyDecision();
		} // MakeAndVerifyDecision
	} // class VerifyReapproval
} // namespace
