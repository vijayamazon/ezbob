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
			var a = new Agent(
				oRow.CustomerId,
				null,
				null,
				Tag,
				DB,
				Log
			).Init();
			
			a.MakeAndVerifyDecision();

			return !a.WasException && !a.WasMismatch;
		} // MakeAndVerifyDecision
	} // class VerifyReject
} // namespace
