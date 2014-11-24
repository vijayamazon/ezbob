namespace EzBob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public abstract class AApprovedVerificationBase : AStrategy {
		#region public

		#region property Name

		public override string Name {
			get { return "Verify " + DecisionName; }
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

				string sResult = VerifyOne(oRow);

				Log.Debug("Customer {0} out of {1}, id {2} complete, result: {3}.",
					i + 1, lst.Count, oRow.CustomerId, sResult
				);
			} // for

			Log.Debug(
				"{4}: sent {0} = mismatch {1} + exception {2} + match {3}.",
				lst.Count, m_nMismatchCount, m_nExceptionCount, m_nMatchCount, DecisionName
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region protected

		#region constructor

		protected AApprovedVerificationBase(int nTopCount, int nLastCheckedCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nTopCount = nTopCount;
			m_nLastCheckedCustomerID = nLastCheckedCustomerID;
		} // constructor

		#endregion constructor

		protected abstract string DecisionName { get; }

		protected abstract bool MakeAndVerifyDecision(AutoApproveInputRow oRow);

		#endregion protected

		#region private

		private string VerifyOne(AutoApproveInputRow oRow) {
			try {
				string sResult;

				if (MakeAndVerifyDecision(oRow)) {
					sResult = "match";
					m_nMatchCount++;
				}
				else {
					sResult = "mismatch";
					m_nMismatchCount++;
				} // if

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
