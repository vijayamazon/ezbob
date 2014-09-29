namespace Reports {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoanIntegrity : SafeLog {
		public LoanIntegrity(AConnection oDB, ASafeLog oLog = null)
			: base(oLog) {
			VerboseLogging = false;
			m_oDB = oDB;
		}

		public SortedDictionary<int, decimal> Run() {
			return CheckLoanIntegrity();
		}

		public bool VerboseLogging { get; set; }

		public SortedDictionary<int, decimal> CheckLoanIntegrity() {
			var res = new SortedDictionary<int, decimal>();

			ConnectionWrapper cw = m_oDB.GetPersistent();

			IEnumerable<SafeReader> loansTbl = m_oDB.ExecuteEnumerable(cw, "SELECT Id, LoanAmount FROM Loan");

			foreach (SafeReader sr in loansTbl) {
				int loanId = sr[0];
				int loanAmount = sr[1];

				decimal paidSoFar = m_oDB.ExecuteScalar<decimal>(cw, string.Format("SELECT CASE WHEN sum(Amount) IS NULL THEN 0 ELSE sum(Amount) END FROM LoanTransaction WHERE LoanId={0} AND Status='Done' AND Type = 'PaypointTransaction'", loanId));

				decimal scheduledToPay = m_oDB.ExecuteScalar<decimal>(cw, string.Format("SELECT CASE WHEN sum(AmountDue) IS NULL THEN 0 ELSE sum(AmountDue) END FROM LoanSchedule WHERE LoanId={0} AND (Status='StillToPay' OR Status='Late')", loanId));

				decimal simpleDiff = loanAmount - paidSoFar - scheduledToPay;

				if (simpleDiff > 0)
					Info("Alert!!! Loan:{0} has an error. LoanAmount:{1}, paidSoFar:{2} ScheduledToPay:{3}", loanId, loanAmount, paidSoFar, scheduledToPay);

				res.Add(loanId, simpleDiff);
			} // for each

			cw.Close();

			return res;
		} // CheckLoanIntegrity

		private readonly AConnection m_oDB;
	}
}
