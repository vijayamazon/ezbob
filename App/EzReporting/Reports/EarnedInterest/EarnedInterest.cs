using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ezbob.Database;
using Ezbob.Logger;
using Ezbob.ValueIntervals;

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
			ByIssuedLoans,

			/// <summary>
			/// Calculates earned interest for all loans that were issued
			/// to customers that are currently (i.e. at run time) are
			/// marked as CCI. Earned interest is calculated
			/// for the date range between the earliest issued loan
			/// and today.
			/// </summary>
			CciCustomers,

			/// <summary>
			/// Calculates earned interest for all the loans that were live
			/// during report period. For each loan earned interest is from
			/// the loan issued date (even if it is outside report period)
			/// till the end of report period.
			/// </summary>
			AccountingLoanBalance,
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
			m_oFreezePeriods = new SortedDictionary<int, InterestFreezePeriods>();

			m_nMode = nMode;
		} // constructor

		#endregion constructor

		#region method Run

		public SortedDictionary<int, decimal> Run() {
			switch (m_nMode) {
			case WorkingMode.ByIssuedLoans:
				FillBySp("RptEarnedInterest_IssuedLoans", false, false);
				break;

			case WorkingMode.ForPeriod:
				FillForPeriod();
				break;

			case WorkingMode.CciCustomers:
				FillBySp("RptEarnedInterest_CciCustomers", false, false);
				break;

			case WorkingMode.AccountingLoanBalance:
				FillBySp("RptEarnedInterest_ForPeriod", true, true);
				break;

			default:
				throw new NotImplementedException("Unsupported working mode: " + m_nMode.ToString());
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
			DataTable tbl = m_oDB.ExecuteReader("RptEarnedInterest_Freeze", CommandSpecies.StoredProcedure);
			
			foreach (DataRow row in tbl.Rows) {
				int nLoanID = Convert.ToInt32(row["LoanId"]);
				DateTime? oStart = row["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["StartDate"]);
				DateTime? oEnd = row["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["EndDate"]);
				decimal nRate = Convert.ToDecimal(row["InterestRate"]);
				DateTime? oDeactivation = row["DeactivationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DeactivationDate"]);

				DateTime? oTo = oDeactivation.HasValue
					? (oEnd.HasValue ? DateInterval.Min(oEnd.Value, oDeactivation.Value) : oDeactivation)
					: oEnd;

				if (!m_oFreezePeriods.ContainsKey(nLoanID))
					m_oFreezePeriods[nLoanID] = new InterestFreezePeriods();

				m_oFreezePeriods[nLoanID].Add(oStart, oTo, nRate);
			} // for each
		} // FillFreezeIntervals

		#endregion method FillFreezeIntervals

		#region method ProcessLoans

		private SortedDictionary<int, decimal> ProcessLoans() {
			DataTable tbl = m_oDB.ExecuteReader("RptEarnedInterest_LoanDates");

			foreach (DataRow row in tbl.Rows) {
				int nLoanID = Convert.ToInt32(row[1]);

				if (!m_oLoans.ContainsKey(nLoanID)) {
					if (VerboseLogging)
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
				InterestFreezePeriods ifp = m_oFreezePeriods.ContainsKey(pair.Key) ? m_oFreezePeriods[pair.Key] : null;

				decimal nInterest = pair.Value.Calculate(m_oDateStart, m_oDateEnd, ifp, VerboseLogging, m_nMode);

				if (nInterest > 0)
					oRes[pair.Key] = nInterest;
			} // foreach

			return oRes;
		} // ProcessLoans

		#endregion method ProcessLoans

		#region method FillForPeriod

		private void FillForPeriod() {
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

		#region method FillBySp

		private void FillBySp(string sSpName, bool bKeepStartDate, bool bKeepEndDate) {
			DataTable tbl = m_oDB.ExecuteReader(
				sSpName,
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", m_oDateStart),
				new QueryParameter("@DateEnd", m_oDateEnd)
			);

			DateTime oDateStart = DateTime.Now.AddYears(1980);

			if (!bKeepEndDate)
				m_oDateEnd = DateTime.Today.AddDays(1);

			foreach (DataRow row in tbl.Rows) {
				int nLoanID = Convert.ToInt32(row[0]);
				DateTime oDate = Convert.ToDateTime(row[1]);
				decimal nAmount = Convert.ToDecimal(row[2]);

				if (oDate < oDateStart)
					oDateStart = oDate;

				m_oLoans[nLoanID] = new LoanData(nLoanID, this) {
					IssueDate = oDate,
					Amount = nAmount
				};
			} // for each row

			if (!bKeepStartDate)
				m_oDateStart = oDateStart;

			Info("{0} loans, date range: {1} - {2}", m_oLoans.Count, m_oDateStart, m_oDateEnd);
		} // FillBySp

		#endregion method FillBySp

		#region fields

		private readonly SortedDictionary<int, LoanData> m_oLoans;
		private readonly SortedDictionary<int, InterestFreezePeriods> m_oFreezePeriods;

		private DateTime m_oDateStart;
		private DateTime m_oDateEnd;

		private readonly AConnection m_oDB;

		private readonly WorkingMode m_nMode;

		#endregion fields

		#endregion private
	} // class EarnedInterest

	#endregion class EarnedInterest
} // namespace Reports
