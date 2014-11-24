namespace EzBob.Backend.Strategies.AutomationVerification {
	using System;
	using AutomationCalculator.ProcessHistory.Common;
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions;
	using EzBob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class VerifyApproval : AApprovedVerificationBase {
		#region public

		public VerifyApproval(
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
			get { return "Auto approval"; }
		} // DecisionName

		#endregion property DecisionName

		#region method MakeAndVerifyDecision

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			return new Approval(
				oRow.CustomerId,
				oRow.OfferedLoanAmount,
				oRow.GetMedal(),
				DB,
				Log
			).Init().MakeAndVerifyDecision();
		} // MakeAndVerifyDecision

		#endregion method MakeAndVerifyDecision

		#endregion protected
	} // class VerifyApproval
} // namespace
