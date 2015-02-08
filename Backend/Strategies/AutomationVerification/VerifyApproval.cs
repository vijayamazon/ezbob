﻿namespace Ezbob.Backend.Strategies.AutomationVerification {
	using Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.Approval;

	public class VerifyApproval : AVerificationBase {
		public VerifyApproval(int nTopCount, int nLastCheckedCustomerID) : base(nTopCount, nLastCheckedCustomerID) {}

		protected override string DecisionName {
			get { return "Auto approval"; }
		} // DecisionName

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			return new Approval(
				oRow.CustomerId,
				oRow.OfferedLoanAmount,
				oRow.GetMedal(),
				oRow.GetMedalType(),
				oRow.GetTurnoverType(),
				DB,
				Log
			).Init().MakeAndVerifyDecision();
		} // MakeAndVerifyDecision
	} // class VerifyApproval
} // namespace
