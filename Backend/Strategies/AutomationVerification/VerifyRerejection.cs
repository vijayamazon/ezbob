namespace EzBob.Backend.Strategies.AutomationVerification {
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class VerifyRerejection : AVerificationBase {
		#region public

		public VerifyRerejection(
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
			get { return "Auto re-rejection"; }
		} // DecisionName

		#endregion property DecisionName

		#region method MakeAndVerifyDecision

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			return new ReRejection(
				oRow.CustomerId,
				DB,
				Log
			).MakeAndVerifyDecision();
		} // MakeAndVerifyDecision

		#endregion method MakeAndVerifyDecision

		#endregion protected
	} // class VerifyRerejection
} // namespace
