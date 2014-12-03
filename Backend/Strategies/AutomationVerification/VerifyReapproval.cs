namespace EzBob.Backend.Strategies.AutomationVerification {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class VerifyReapproval : AVerificationBase {
		#region public

		public VerifyReapproval(
			int nTopCount,
			int nLastCheckedCustomerID,
			AConnection oDB,
			ASafeLog oLog
		) : base(nTopCount, nLastCheckedCustomerID, oDB, oLog) {
		} // constructor

		#endregion public

		#region protected

		#region property DecisionName

		protected override string DecisionName {
			get { return "Auto Re-approval"; }
		} // DecisionName

		#endregion property DecisionName

		#region method MakeAndVerifyDecision

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			return new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval.Agent(
				oRow.CustomerId,
				DB,
				Log
			).Init().MakeAndVerifyDecision();
		} // MakeAndVerifyDecision

		#endregion method MakeAndVerifyDecision

		#endregion protected
	} // class VerifyReapproval
} // namespace
