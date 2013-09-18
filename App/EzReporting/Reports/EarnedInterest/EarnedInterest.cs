using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ezbob.Database;
using Ezbob.Logger;

namespace Reports {
	#region class EarnedInterest

	public class EarnedInterest : SafeLog {
		#region public

		#region enum WorkingMode

		public enum WorkingMode {
			/// <summary>
			/// Calculates earned interest for specified period
			/// over all the loans.
			/// </summary>
			ForPeriod,

			/// <summary>
			/// Calculates earned interest for loans that were issued
			/// during specified period. Earned interest is calculated
			/// for the date range between the earliest issued loan
			/// and today.
			/// </summary>
			ByIssuedLoans
		} // enum WorkingMode

		#endregion enum WorkingMode

		#region constructor

		public EarnedInterest(AConnection oDB, WorkingMode nMode, DateTime oDateOne, DateTime oDateTwo, ASafeLog oLog = null) : base(oLog) {
			VerboseLogging = false;

			m_oDB = oDB;

			if (oDateTwo < oDateOne) {
				DateTime tmp = oDateOne;
				oDateOne = oDateTwo;
				oDateTwo = tmp;
			} // if

			m_oDateStart = oDateOne;
			m_oDateEnd = oDateTwo;

			m_oLoans = new SortedDictionary<int, LoanData>();

			m_nMode = nMode;
		} // constructor

		#endregion constructor

		#region method Run

		public SortedDictionary<int, decimal> Run() {
			switch (m_nMode) {
			case WorkingMode.ByIssuedLoans:
				FillIssuedLoans();
				break;

			case WorkingMode.ForPeriod:
				FillForPeriod();
				break;
			} // switch

			FillFreezeIntervals();

			return ProcessLoans();
		} // Run
 
		#endregion method Run

		#region property VerboseLogging

		public bool VerboseLogging { get; set; }

		#endregion property VerboseLogging

		#endregion public

		#region private

		#region method FillFreezeIntervals

		private void FillFreezeIntervals() {
			// TODO
			/*
			DataTable tbl = m_oDB.ExecuteReader(
				"RptEarnedInterest_ForPeriod", 
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", m_oDateStart),
				new QueryParameter("@DateEnd", m_oDateEnd)
			);
			*/
		} // FillFreezeIntervals

		#endregion method FillFreezeIntervals

		#region method ProcessLoans

		private SortedDictionary<int, decimal> ProcessLoans() {
			DataTable tbl = m_oDB.ExecuteReader("RptEarnedInterest_LoanDates");

			foreach (DataRow row in tbl.Rows) {
				int nLoanID = Convert.ToInt32(row[1]);

				if (!m_oLoans.ContainsKey(nLoanID)) {
					Debug("Ignoring loan id {0}", nLoanID);
					continue;
				} // if

				DateTime oDate = Convert.ToDateTime(row[2]);

				decimal nValue = Convert.ToDecimal(row[3]);

				switch (row[0].ToString()) {
				case "0":
					m_oLoans[nLoanID].Schedule[oDate] = new InterestData(oDate, nValue);

					break;

				case "1":
					if (nValue > 0)
						m_oLoans[nLoanID].Repayments[oDate] = new TransactionData(oDate, nValue);

					break;
				} // switch
			} // for each row

			var oRes = new SortedDictionary<int, decimal>();

			foreach (KeyValuePair<int, LoanData> pair in m_oLoans) {
				decimal nInterest = pair.Value.Calculate(m_oDateStart, m_oDateEnd, VerboseLogging);

				if (nInterest > 0)
					oRes[pair.Key] = nInterest;
			} // foreach

			return oRes;
		} // ProcessLoans

		#endregion method ProcessLoans

		#region method FillIssuedLoans

		public void FillIssuedLoans() {
			DataTable tbl = m_oDB.ExecuteReader(
				"RptEarnedInterest_IssuedLoans", 
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", m_oDateStart),
				new QueryParameter("@DateEnd", m_oDateEnd)
			);

			m_oDateStart = DateTime.Now.AddYears(1980);
			m_oDateEnd = DateTime.Today.AddDays(1);

			foreach (DataRow row in tbl.Rows) {
				int nLoanID = Convert.ToInt32(row[0]);
				DateTime oDate = Convert.ToDateTime(row[1]);
				decimal nAmount = Convert.ToDecimal(row[2]);

				if (oDate < m_oDateStart)
					m_oDateStart = oDate;

				m_oLoans[nLoanID] = new LoanData(nLoanID, this) {
					IssueDate = oDate,
					Amount = nAmount
				};
			} // for each row

			Info("{0} loans, date range: {1} - {2}", m_oLoans.Count, m_oDateStart, m_oDateEnd);
		} // FillIssuedLoans

		#endregion method FillIssuedLoans

		#region method FillForPeriod

		public void FillForPeriod() {
			DataTable tbl = m_oDB.ExecuteReader(
				"RptEarnedInterest_ForPeriod", 
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", m_oDateStart),
				new QueryParameter("@DateEnd", m_oDateEnd)
			);

			foreach (DataRow row in tbl.Rows) {
				int nLoanID = Convert.ToInt32(row[0]);
				DateTime oDate = Convert.ToDateTime(row[1]);
				decimal nAmount = Convert.ToDecimal(row[2]);

				m_oLoans[nLoanID] = new LoanData(nLoanID, this) {
					IssueDate = oDate,
					Amount = nAmount
				};
			} // for each row

			Info("{0} loans, date range: {1} - {2}", m_oLoans.Count, m_oDateStart, m_oDateEnd);
		} // FillForPeriod

		#endregion method FillForPeriod

		#region fields

		private readonly SortedDictionary<int, LoanData> m_oLoans;

		private DateTime m_oDateStart;
		private DateTime m_oDateEnd;

		private readonly AConnection m_oDB;

		private readonly WorkingMode m_nMode;

		#endregion fields

		#endregion private
	} // class EarnedInterest

	#endregion class EarnedInterest
} // namespace Reports
