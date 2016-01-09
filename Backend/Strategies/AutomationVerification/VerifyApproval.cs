﻿namespace Ezbob.Backend.Strategies.AutomationVerification {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval;

	public class VerifyApproval : AVerificationBase {
		public VerifyApproval(int nTopCount, int nLastCheckedCustomerID) : base(nTopCount, nLastCheckedCustomerID) {}

		protected override string DecisionName {
			get { return "Auto approval"; }
		} // DecisionName

		protected override bool MakeAndVerifyDecision(AutoApproveInputRow oRow) {
			var a = new Approval(
				oRow.CustomerId,
				null,
				oRow.OfferedLoanAmount,
				oRow.GetMedal(),
				oRow.GetMedalType(),
				oRow.GetTurnoverType(),
				DB,
				Log
			).Init();
			
			a.MakeAndVerifyDecision(Tag);

			return !a.WasMismatch;
		} // MakeAndVerifyDecision
	} // class VerifyApproval
} // namespace
