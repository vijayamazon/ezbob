namespace Ezbob.Backend.Strategies.AutomationVerification {
	using Ezbob.Backend.Strategies.MainStrategy.AutoDecisions;

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
			return new ReRejection(
				oRow.CustomerId,
				DB,
				Log
			).MakeAndVerifyDecision();
		} // MakeAndVerifyDecision

	} // class VerifyRerejection
} // namespace
