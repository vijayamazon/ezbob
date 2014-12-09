namespace EzBob.Backend.Strategies.AutomationVerification {
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class VerifyApproval : AVerificationBase {

		public VerifyApproval(
			int nTopCount,
			int nLastCheckedCustomerID,
			AConnection oDB,
			ASafeLog oLog
		) : base(nTopCount, nLastCheckedCustomerID, oDB, oLog) {
		} // constructor

		protected override string DecisionName {
			get { return "Auto approval"; }
		} // DecisionName

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			return new Approval(
				oRow.CustomerId,
				oRow.OfferedLoanAmount,
				oRow.GetMedal(),
				DB,
				Log
			).Init().MakeAndVerifyDecision();
		} // MakeAndVerifyDecision

	} // class VerifyApproval
} // namespace
