namespace EzBob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory.Common;
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class VerifyReapproval : AStrategy {
		#region public

		#region constructor

		public VerifyReapproval(int nTopCount, int nLastCheckedCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nTopCount = nTopCount;
			m_nLastCheckedCustomerID = nLastCheckedCustomerID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "VerifyReapproval"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_nExceptionCount = 0;
			m_nMatchCount = 0;
			m_nMismatchCount = 0;

			List<AutoApproveInputRow> lst = AutoApproveInputRow.Load(DB, m_nTopCount, m_nLastCheckedCustomerID);

			for (int i = 0; i < lst.Count; i++) {
				AutoApproveInputRow oRow = lst[i];

				Log.Debug("Customer {0} out of {1}, id {2}...", i + 1, lst.Count, oRow.CustomerId);

				string sResult = VerifyOne(oRow.CustomerId);

				Log.Debug("Customer {0} out of {1}, id {2} complete, result: {3}.",
					i + 1, lst.Count, oRow.CustomerId, sResult
				);
			} // for

			Log.Debug(
				"Auto re-approval: sent {0} = mismatch {1} + exception {2} + match {3}.",
				lst.Count, m_nMismatchCount, m_nExceptionCount, m_nMatchCount
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private string VerifyOne(int nCustomerID) {
			try {
				string sResult;

				var autoDecisionResponse = new AutoDecisionResponse {DecisionName = "Manual"};

				var oReapprove = new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval.Agent(
					nCustomerID,
					DB,
					Log
				);

				oReapprove.MakeDecision(autoDecisionResponse);

				var oSecondary = new AutomationCalculator.AutoDecision.AutoReApproval.Agent(Log, nCustomerID, oReapprove.Trail.InputData.DataAsOf);
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

					sResult = "match";
					m_nMatchCount++;
				}
				else {
					sResult = "mismatch";
					m_nMismatchCount++;
				} // if

				oReapprove.Trail.Save(DB, oSecondary.Trail);

				return sResult;
			}
			catch (Exception e) {
				Log.Debug(e, "Exception caught.");
				m_nExceptionCount++;
				return "exception";
			} // try
		} // VerifyOne

		private readonly int m_nTopCount;
		private readonly int m_nLastCheckedCustomerID;

		private int m_nMatchCount;
		private int m_nMismatchCount;
		private int m_nExceptionCount;

		#endregion private
	} // class VerifyReapproval
} // namespace
