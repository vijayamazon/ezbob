namespace Ezbob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	public abstract class AVerificationBase : AStrategy {
		public override string Name {
			get { return "Verify " + DecisionName; }
		} // Name

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

				Log.Debug(
					"{4}: total {0}, sent {5} = mismatch {1} + exception {2} + match {3}.",
					lst.Count, m_nMismatchCount, m_nExceptionCount, m_nMatchCount, DecisionName, i + 1
				);
			} // for
		} // Execute

		protected abstract string DecisionName { get; }

		protected virtual string Tag {
			get {
				if (this.tag != null)
					return this.tag;

				this.tag = "#Verify" + DecisionName + "_" +
					DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture) + "_" +
					Guid.NewGuid().ToString("N");

				return this.tag;
			} // get
		} // Tag

		protected AVerificationBase(int nTopCount, int nLastCheckedCustomerID) {
			m_nTopCount = nTopCount;
			m_nLastCheckedCustomerID = nLastCheckedCustomerID;
		} // constructor

		protected abstract bool MakeAndVerifyDecision(AutoApproveInputRow oRow);

		private string VerifyOne(AutoApproveInputRow oRow) {
			try {
				string sResult;

				if (MakeAndVerifyDecision(oRow)) {
					sResult = "match";
					m_nMatchCount++;
				} else {
					sResult = "mismatch";
					m_nMismatchCount++;
				} // if

				return sResult;
			} catch (Exception e) {
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

		private string tag;
	} // class AVerificationBase
} // namespace
