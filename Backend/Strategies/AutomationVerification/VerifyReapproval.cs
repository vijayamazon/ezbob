namespace EzBob.Backend.Strategies.AutomationVerification {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class VerifyReapproval : AVerificationBase {

		public VerifyReapproval(
			int nTopCount,
			int nLastCheckedCustomerID,
			AConnection oDB,
			ASafeLog oLog
		) : base(nTopCount, nLastCheckedCustomerID, oDB, oLog) {
		} // constructor

		protected override string DecisionName {
			get { return "Auto Re-approval"; }
		} // DecisionName

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			return new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval.Agent(
				oRow.CustomerId,
				DB,
				Log
			).Init().MakeAndVerifyDecision();
		} // MakeAndVerifyDecision

	} // class VerifyReapproval
} // namespace
