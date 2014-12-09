namespace EzBob.Backend.Strategies.AutomationVerification {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class VerifyReject : AVerificationBase {

		public VerifyReject(
			int nTopCount,
			int nLastCheckedCustomerID,
			AConnection oDB,
			ASafeLog oLog
		) : base(nTopCount, nLastCheckedCustomerID, oDB, oLog) {
		} // constructor

		protected override string DecisionName {
			get { return "Auto reject"; }
		} // DecisionName

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			return new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Agent(
				oRow.CustomerId,
				DB,
				Log
			).Init().MakeAndVerifyDecision();
		} // MakeAndVerifyDecision

	} // class VerifyReject
} // namespace
