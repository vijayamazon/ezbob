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

			return ProcessLoans();
		} // Run
 
		#endregion method Run

		#endregion public

		#region private

		#region method ProcessLoans

		public SortedDictionary<int, decimal> ProcessLoans() {
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
				decimal nInterest = pair.Value.Calculate(m_oDateStart, m_oDateEnd);

				if (nInterest > 0)
					oRes[pair.Key] = nInterest;
			} // foreach

			return oRes;
		} // ProcessLoans

		#endregion method ProcessLoans

		#region class LoanData

		private class LoanData {
			public DateTime IssueDate;
			public decimal Amount;
			public readonly SortedDictionary<DateTime, InterestData> Schedule;
			public readonly SortedDictionary<DateTime, TransactionData> Repayments;

			public LoanData(int nLoanID, ASafeLog oLog) {
				LoanID = nLoanID;
				Log = new SafeLog(oLog);

				Schedule = new SortedDictionary<DateTime, InterestData>();
				Repayments = new SortedDictionary<DateTime, TransactionData>();
			} // constructor

			#region class PrInterest

			private class PrInterest {
				public readonly DateTime Date;
				public decimal Principal;
				public decimal Interest;

				public PrInterest(DateTime oDate, decimal nPrincipal) {
					Date = oDate;
					Principal = nPrincipal;
					Interest = 0;
				} // constructor

				public override string ToString() {
					return string.Format("{0}: {1} * {2} = {3}", Date, Principal, Interest, Principal * Interest);
				} // ToString

				public bool Update(InterestData oDelta) {
					Interest = oDelta.Interest;

					return Date == oDelta.Date;
				} // Update

				public void Update(TransactionData oDelta) {
					if (Date > oDelta.Date)
						Principal -= oDelta.Repayment;
				} // Update
			} // class PrInterest

			#endregion class PrInterest

			#region method Calculate

			/// <summary>
			/// Earned interest is a SUM(Pj * Ij) where the sum is taken on all the days during
			/// requested period when a loan could produce interest, Pj is a loan principal on
			/// specific day, Ij is an interest on that day.
			/// </summary>
			/// <param name="oDateStart">Requested period start date, inclusive.</param>
			/// <param name="oDateEnd">Requested period end date, exclusive.</param>
			/// <returns>Earned interest for the period.</returns>
			public decimal Calculate(DateTime oDateStart, DateTime oDateEnd) {
				DateTime oFirstIncomeDay = IssueDate.AddDays(1);

				// A loan starts to produce interest on the next day.
				DateTime oDayOne = (oDateStart < oFirstIncomeDay) ? oFirstIncomeDay : oDateStart;

				// List of all the dates during requested period when a loan could produce interest.
				var oDaysList = new List<PrInterest>();

				for (DateTime d = oDayOne; d < oDateEnd; d = d.AddDays(1))
					oDaysList.Add(new PrInterest(d, Amount));

				if (oDaysList.Count == 0)
					return 0;

				DateTime oPrevDate = IssueDate;

				foreach (InterestData ida in Schedule.Values) {
					ida.PeriodLength = (ida.Date - oPrevDate).Days;
					oPrevDate = ida.Date;
				} // for each schedule

				oDaysList.ForEach(pri => {
					foreach (TransactionData t in Repayments.Values)
						pri.Update(t);
				}); // for each day

				PrInterest[] days = oDaysList.Where(pri => pri.Principal > 0).ToArray();

				if (days.Length == 0)
					return 0;

				InterestData[] aryDates = Schedule.Values.ToArray();

				int nDayDataPtr = 0;
				InterestData oCurDayData = aryDates[nDayDataPtr];
				
				foreach (PrInterest pri in days) {
					if (pri.Update(oCurDayData)) {
						nDayDataPtr++;

						if (nDayDataPtr < aryDates.Length)
							oCurDayData = aryDates[nDayDataPtr];
					} // if
				} // for

				decimal nEarnedInterest = days.Sum(pri => pri.Principal * pri.Interest);

				// Log.Debug("\n\nLoanID: {0}, {1} issued on {2} earned interest is {6}\nSchedule ({7}):\n\t{3}\nTransactions ({8}):\n\t{4}\nPer day:\n\t{5}\n\n",
					// LoanID, Amount, IssueDate,
					// string.Join(" ; ", Schedule.Values.Select(v => v.ToString()).ToArray()),
					// string.Join(" ; ", Repayments.Values.Select(v => v.ToString()).ToArray()),
					// string.Join("\n\t", days.Select(pri => pri.ToString()).ToArray()),
					// nEarnedInterest,
					// Schedule.Count,
					// Repayments.Count
				// );

				return nEarnedInterest;
			} // Calculate

			#endregion method Calculate

			#region private

			private readonly int LoanID;
			private readonly ASafeLog Log;

			#endregion private
		} // LoanData

		#endregion class LoanData

		#region class InterestData

		private class InterestData {
			#region public

			public readonly DateTime Date;
			public int PeriodLength;
			public readonly decimal OriginalInterest;

			#region constructor

			public InterestData(DateTime oDate, decimal nInterest) {
				PeriodLength = 0;
				Date = oDate;
				OriginalInterest = nInterest;
			} // constructor

			#endregion constructor

			#region property Interest

			public decimal Interest {
				get { return PeriodLength == 0 ? OriginalInterest : OriginalInterest / PeriodLength; }
			} // Interest

			#endregion property Interest

			#region method ToStirng

			public override string ToString() {
				return string.Format("on {0}: {1} = {2} / {3}", Date, Interest, OriginalInterest, PeriodLength);
			} // ToString

			#endregion method ToStirng

			#endregion public
		} // class InterestData

		#endregion class InterestData

		#region class TransactionData

		private class TransactionData {
			#region public

			public readonly DateTime Date;
			public decimal Repayment;

			#region constructor

			public TransactionData(DateTime oDate, decimal nRepayment) {
				Date = oDate;
				Repayment = nRepayment;
			} // constructor

			#endregion constructor

			#region method ToStirng

			public override string ToString() {
				return string.Format("on {0}: {1}", Date, Repayment);
			} // ToString

			#endregion method ToStirng

			#endregion public
		} // class TransactionData

		#endregion class TransactionData

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
