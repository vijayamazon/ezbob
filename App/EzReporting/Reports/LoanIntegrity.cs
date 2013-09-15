namespace Reports 
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoanIntegrity : SafeLog 
	{
		public LoanIntegrity(AConnection oDB, ASafeLog oLog = null) : base(oLog) 
		{
			VerboseLogging = false;
			m_oDB = oDB;
		}

		public SortedDictionary<int, decimal> Run() 
		{
			return CheckLoanIntegrity();
		}

		public bool VerboseLogging { get; set; }

		public SortedDictionary<int, decimal> CheckLoanIntegrity() 
		{
			var res = new SortedDictionary<int, decimal>();
			DataTable loansTbl = m_oDB.ExecuteReader("SELECT Id, LoanAmount FROM Loan");

			foreach (DataRow row in loansTbl.Rows)
			{
				int loanId = Convert.ToInt32(row[0]);
				int loanAmount = Convert.ToInt32(row[1]);

				DataTable transactionsTbl = m_oDB.ExecuteReader(string.Format("SELECT CASE WHEN sum(Amount) IS NULL THEN 0 ELSE sum(Amount) END FROM LoanTransaction WHERE LoanId={0} AND Status='Done' AND Type = 'PaypointTransaction'", loanId));
				decimal paidSoFar = Convert.ToDecimal(transactionsTbl.Rows[0][0]);

				DataTable scheduleTbl = m_oDB.ExecuteReader(string.Format("SELECT CASE WHEN sum(AmountDue) IS NULL THEN 0 ELSE sum(AmountDue) END FROM LoanSchedule WHERE LoanId={0} AND (Status='StillToPay' OR Status='Late')", loanId));
				decimal scheduledToPay = Convert.ToDecimal(scheduleTbl.Rows[0][0]);

				decimal simpleDiff = loanAmount - paidSoFar - scheduledToPay;

				if (simpleDiff > 0)
					Info("Alert!!! Loan:{0} has an error. LoanAmount:{1}, paidSoFar:{2} ScheduledToPay:{3}", loanId, loanAmount,
					     paidSoFar, scheduledToPay);

				res.Add(loanId, simpleDiff);
			}

			return res;
		}

		private readonly AConnection m_oDB;
	}
}
