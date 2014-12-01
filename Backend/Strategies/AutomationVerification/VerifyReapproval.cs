namespace EzBob.Backend.Strategies.AutomationVerification {
	using AutomationCalculator.ProcessHistory.Common;
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions;
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
			var autoDecisionResponse = new AutoDecisionResponse {DecisionName = "Manual"};

			var oReapprove = new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval.Agent(
				oRow.CustomerId,
				DB,
				Log
			).Init();
			
			oReapprove.MakeDecision(autoDecisionResponse);

			var oSecondary = new AutomationCalculator.AutoDecision.AutoReApproval.Agent(
				DB, Log, oRow.CustomerId, oReapprove.Trail.InputData.DataAsOf
			);

			oSecondary.MakeDecision(oSecondary.GetInputData());

			bool bSuccess = oReapprove.Trail.EqualsTo(oSecondary.Trail);

			if (bSuccess) {
				if (oReapprove.Trail.HasDecided) {
					if (oReapprove.ApprovedAmount == oSecondary.Result.ReApproveAmount) {
						oReapprove.Trail.Affirmative<SameAmount>(false).Init(oReapprove.ApprovedAmount);
						oSecondary.Trail.Affirmative<SameAmount>(false).Init(oSecondary.Result.ReApproveAmount);
					}
					else {
						oReapprove.Trail.Negative<SameAmount>(false).Init(oReapprove.ApprovedAmount);
						oSecondary.Trail.Negative<SameAmount>(false).Init(oSecondary.Result.ReApproveAmount);
						bSuccess = false;
					} // if
				} // if
			} // if

			oReapprove.Trail.Save(DB, oSecondary.Trail);

			return bSuccess;
		} // MakeAndVerifyDecision

		#endregion method MakeAndVerifyDecision

		#endregion protected
	} // class VerifyReapproval
} // namespace
