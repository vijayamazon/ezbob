namespace EzBob.Backend.Strategies.AutomationVerification {
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class VerifyRerejection : AVerificationBase {

		public VerifyRerejection(
			int nTopCount,
			int nLastCheckedCustomerID,
			AConnection oDB,
			ASafeLog oLog
		) : base(nTopCount, nLastCheckedCustomerID, oDB, oLog) {
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
