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

		public SortedDictionary<int, double> Run() {
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

		public SortedDictionary<int, double> ProcessLoans() {
			DataTable tbl = m_oDB.ExecuteReader("RptEarnedInterest_LoanDates");

			foreach (DataRow row in tbl.Rows) {
				int nLoanID = Convert.ToInt32(row[1]);

				if (!m_oLoans.ContainsKey(nLoanID)) {
					Debug("Ignoring loan id {0}", nLoanID);
					continue;
				} // if

				DataItem nItem = DataItem.Unknown;

				switch (row[0].ToString()) {
				case "0":
					nItem = DataItem.Schedule;
					break;

				case "1":
					nItem = DataItem.Transaction;
					break;
				} // switch

				DateTime oDate = Convert.ToDateTime(row[2]);

				double nValue = Convert.ToDouble(row[3]);

				m_oLoans[nLoanID].Add(new DayData(nItem, oDate, nValue));
			} // for each row

			var oRes = new SortedDictionary<int, double>();

			foreach (KeyValuePair<int, LoanData> pair in m_oLoans)
				oRes[pair.Key] = pair.Value.Calculate(m_oDateStart, m_oDateEnd);

			return oRes;
		} // ProcessLoans

		#endregion method ProcessLoans

		#region enum DataItem

		private enum DataItem {
			Unknown,
			Schedule,
			Transaction
		} // DataItem

		#endregion enum DataItem

		#region class LoanData

		private class LoanData {
			public DateTime IssueDate;
			public double Amount;

			public LoanData(int nLoanID, ASafeLog oLog) {
				LoanID = nLoanID;
				Log = new SafeLog(oLog);
				Dates = new SortedDictionary<DateTime, DayData>();
			} // constructor

			#region method Add

			public void Add(DayData oDay) {
				if (Dates.ContainsKey(oDay.Date))
					Dates[oDay.Date].Update(oDay);
				else
					Dates[oDay.Date] = oDay;
			} // Add

			#endregion method Add

			#region class PrInterest

			private class PrInterest {
				public readonly DateTime Date;
				public double Principal;
				public double Interest;

				public PrInterest(DateTime oDate, double nPrincipal) {
					Date = oDate;
					Principal = nPrincipal;
					Interest = 0;
				} // constructor

				public override string ToString() {
					return string.Format("{0}: {1} -- {2}", Date, Principal, Interest);
				} // ToString

				public void DecPrincipal(DayData oDelta) {
					if ((oDelta.Repayment > 0) && (Date > oDelta.Date))
						Principal -= oDelta.Repayment;
				} // DecPrincipal
			} // class PrInterest

			#endregion class PrInterest

			#region class PeriodLengthData

			private class PeriodLengthData {
				public DateTime Date;
				public int Length;
			} // class PeriodLengthData

			#endregion class PeriodLengthData

			#region method Calculate

			public double Calculate(DateTime oDateStart, DateTime oDateEnd) {
				if (Dates.Count == 0)
					return 0;

				DateTime oFirstIncomeDay = IssueDate.AddDays(1);

				DateTime d = (oDateStart < oFirstIncomeDay) ? oFirstIncomeDay : oDateStart;

				var oDaysList = new List<PrInterest>();

				while (d < oDateEnd) {
					oDaysList.Add(new PrInterest(d, Amount));

					d = d.AddDays(1);
				} // while

				if (oDaysList.Count == 0)
					return 0;

				DayData[] aryDates = Dates.Values.ToArray();

				var oSchedules = new List<PeriodLengthData> {
					new PeriodLengthData {
						Date = IssueDate,
						Length = 0
					}
				};

				oSchedules.AddRange(
					from dd in aryDates
					where dd.Item == DataItem.Schedule
					select new PeriodLengthData {
						Date = dd.Date,
						Length = 0
					}
				);

				PeriodLengthData[] arySchedules = oSchedules.ToArray();

				for (int i = arySchedules.Length - 1; i > 0; i--) {
					PeriodLengthData cur = arySchedules[i];
					PeriodLengthData prev = arySchedules[i - 1];

					cur.Length = (cur.Date - prev.Date).Days;
				} // for

				int nSchedPtr = 1;
				PeriodLengthData pld = arySchedules[nSchedPtr];

				for (int i = 0; i < aryDates.Length; i++) {
					DayData oCurDate = aryDates[i];

					if (oCurDate.Date == pld.Date) {
						oCurDate.OriginalInterest = oCurDate.Interest;
						oCurDate.Interest /= pld.Length;

						nSchedPtr++;

						if (nSchedPtr >= arySchedules.Length)
							break;
						else
							pld = arySchedules[nSchedPtr];
					} // if
				} // for

				for (int i = aryDates.Length - 1; i > 0; i--) {
					if ((aryDates[i].Interest >= 0) && (aryDates[i - 1].Interest < 0))
						aryDates[i - 1].Interest = aryDates[i].Interest;
				} // for

				for (int i = 0; i < aryDates.Length - 1; i++) {
					if ((aryDates[i].Interest >= 0) && (aryDates[i + 1].Interest < 0))
						aryDates[i + 1].Interest = aryDates[i].Interest;
				} // for

				oDaysList.ForEach(pri => {
					foreach (DayData dd in aryDates)
						pri.DecPrincipal(dd);
				}); // for each day

				PrInterest[] days = oDaysList.Where(pri => pri.Principal > 0.000001).ToArray();

				if (days.Length == 0)
					return 0;

				int nDayDataPtr = 0;
				DayData oCurDayData = aryDates[nDayDataPtr];
				
				for (int nDayPtr = 0; nDayPtr < days.Length; nDayPtr++) {
					PrInterest pri = days[nDayPtr];

					if (pri.Date <= oCurDayData.Date)
						pri.Interest = oCurDayData.Interest;

					if (pri.Date == oCurDayData.Date) {
						nDayDataPtr++;

						if (nDayDataPtr < aryDates.Length)
							oCurDayData = aryDates[nDayDataPtr];
					} // if
				} // for

				var aryPriStr = days.Select(pri => pri.ToString()).ToArray();

				var aryDayStr = aryDates.Select(dd => dd.ToString()).ToArray();

				Log.Debug("\n\nLoanID: {0}, {1} issued on {2}\nSchedule & Transactions\n{3}\nPer day\n{4}\n\n", LoanID, Amount, IssueDate,
					string.Join(" ; ", aryDayStr),
					string.Join(" ; ", aryPriStr)
				);

				return days.Sum(pri => pri.Principal * pri.Interest);
			} // Calculate

			#endregion method Calculate

			#region private

			private readonly int LoanID;
			private readonly SortedDictionary<DateTime, DayData> Dates;
			private readonly ASafeLog Log;

			#endregion private
		} // LoanData

		#endregion class LoanData

		#region class DayData

		private class DayData {
			#region public

			public DataItem Item;
			public readonly DateTime Date;
			public double Repayment;
			public double Interest;
			public double OriginalInterest;

			#region constructor

			public DayData(DataItem nItem, DateTime oDate, double nValue) {
				Item = nItem;
				Date = oDate;

				switch (nItem) {
				case DataItem.Schedule:
					Repayment = 0;
					Interest = nValue;
					break;

				case DataItem.Transaction:
					Repayment = nValue;
					Interest = -1;
					break;

				default:
					throw new ArgumentOutOfRangeException("nItem");
				} // switch
			} // constructor

			#endregion constructor

			#region method Update

			public void Update(DayData oData) {
				switch (oData.Item) {
				case DataItem.Schedule:
					Interest = oData.Interest;
					Item = DataItem.Schedule;
					break;

				case DataItem.Transaction:
					Repayment = oData.Repayment;
					break;

				default:
					throw new ArgumentOutOfRangeException();
				} // switch
			} // Update

			#endregion method Update

			#region method ToStirng

			public override string ToString() {
				return string.Format("{0} on {1}: {2} -- {3} ({4})", Item, Date, Repayment, Interest, OriginalInterest);
			} // ToString

			#endregion method ToStirng

			#endregion public
		} // DayData

		#endregion class DayData

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
				double nAmount = Convert.ToDouble(row[2]);

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
				double nAmount = Convert.ToDouble(row[2]);

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
