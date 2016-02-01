namespace Ezbob.Backend.Strategies.AutomationVerification {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;

	public class VerifyRerejection : AVerificationBase {
		public VerifyRerejection(
			int nTopCount,
			int nLastCheckedCustomerID
		) : base(nTopCount, nLastCheckedCustomerID) {
		} // constructor

		protected override string DecisionName {
			get { return "Auto re-rejection"; }
		} // DecisionName

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			var agent = new ReRejection(
				oRow.CustomerId,
				null,
				null,
				Tag,
				DB,
				Log
			);
			
			agent.MakeAndVerifyDecision();

			return !agent.WasMismatch;
		} // MakeAndVerifyDecision
	} // class VerifyRerejection
} // namespace
