using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ezbob.Logger;

namespace Reports {
	#region class LoanData

	class LoanData {
		public DateTime IssueDate;
		public decimal Amount;
		public readonly SortedDictionary<DateTime, InterestData> Schedule;
		public readonly SortedDictionary<DateTime, TransactionData> Repayments;

		#region constructor

		public LoanData(int nLoanID, ASafeLog oLog) {
			LoanID = nLoanID;
			Log = new SafeLog(oLog);

			Schedule = new SortedDictionary<DateTime, InterestData>();
			Repayments = new SortedDictionary<DateTime, TransactionData>();
		} // constructor

		#endregion constructor

		#region method Calculate

		/// <summary>
		/// Earned interest is a SUM(Pj * Ij) where the sum is taken on all the days during
		/// requested period when a loan could produce interest, Pj is a loan principal on
		/// specific day, Ij is an interest on that day.
		/// </summary>
		/// <param name="oDateStart">Requested period start date, inclusive.</param>
		/// <param name="oDateEnd">Requested period end date, exclusive.</param>
		/// <param name="ifp">Interest rate freeze periods.</param>
		/// <returns>Earned interest for the period.</returns>
		public decimal Calculate(DateTime oDateStart, DateTime oDateEnd, InterestFreezePeriods ifp, bool bVerboseLogging) {
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
				if (pri.Update(oCurDayData, ifp)) {
					nDayDataPtr++;

					if (nDayDataPtr < aryDates.Length)
						oCurDayData = aryDates[nDayDataPtr];
				} // if
			} // for

			decimal nEarnedInterest = days.Sum(pri => pri.Principal * pri.Interest);

			if (bVerboseLogging) {
				string sIfp = "none";

				if (ifp != null) {
					var os = new StringBuilder();

					ifp.ForEach(i => os.AppendFormat("\n\t{0}", i));

					sIfp = os.ToString().Trim();
				} // if

				Log.Debug("\n\nLoanID: {0}, {1} issued on {2} earned interest is {6}\nSchedule ({7}):\n\t{3}\nFreeze periods ({10}):\n\t{9}\nTransactions ({8}):\n\t{4}\nPer day:\n\t{5}\n\n",
					LoanID, Amount, IssueDate,
					string.Join("\n\t", Schedule.Values.Select(v => v.ToString()).ToArray()),
					string.Join("\n\t", Repayments.Values.Select(v => v.ToString()).ToArray()),
					string.Join("\n\t", days.Select(pri => pri.ToString()).ToArray()),
					nEarnedInterest,
					Schedule.Count,
					Repayments.Count,
					sIfp,
					ifp == null ? 0 : ifp.Count
				);
			} // if

			return nEarnedInterest;
		} // Calculate

		#endregion method Calculate

		#region private

		#region fields

		private readonly int LoanID;
		private readonly ASafeLog Log;

		#endregion fields

		#endregion private
	} // LoanData

	#endregion class LoanData
} // namespace Reports

