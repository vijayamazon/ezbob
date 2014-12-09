namespace Reports.EarnedInterest {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Ezbob.Logger;

	class LoanData {
		public DateTime IssueDate;
		public decimal Amount;
		public int CustomerID;
		public readonly SortedDictionary<DateTime, InterestData> Schedule;
		public readonly SortedDictionary<DateTime, TransactionData> Repayments;

		public LoanData(int nLoanID, ASafeLog oLog) {
			m_nLoanID = nLoanID;
			m_oLog = oLog ?? new SafeLog();

			Schedule = new SortedDictionary<DateTime, InterestData>();
			Repayments = new SortedDictionary<DateTime, TransactionData>();
		} // constructor

		/// <summary>
		/// Earned interest is a SUM(Pj * Ij) where the sum is taken on all the days during
		/// requested period when a loan could produce interest, Pj is a loan principal on
		/// specific day, Ij is an interest on that day.
		/// </summary>
		/// <param name="oDateStart">Requested period start date, inclusive.</param>
		/// <param name="oDateEnd">Requested period end date, exclusive.</param>
		/// <param name="ifp">Interest rate freeze periods.</param>
		/// <param name="bVerboseLogging">Log verbosity level.</param>
		/// <param name="nMode">Interest calculation mode.</param>
		/// <param name="bAccountingMode">How bad statuses and Write Off are treated.</param>
		/// <param name="oWriteOffDate">Date of the first customer's WriteOff status.</param>
		/// <param name="bp">List of customer's bad periods (i.e. when customer was in one of the bad statuses).</param>
		/// <returns>Earned interest for the period.</returns>
		public decimal Calculate(
			DateTime oDateStart,
			DateTime oDateEnd,
			InterestFreezePeriods ifp,
			bool bVerboseLogging,
			Reports.EarnedInterest.EarnedInterest.WorkingMode nMode,
			bool bAccountingMode,
			BadPeriods bp,
			DateTime? oWriteOffDate
		) {
			DateTime oFirstIncomeDay = IssueDate.AddDays(1);

			// A loan starts to produce interest on the next day.

			DateTime oDayOne = (nMode == Reports.EarnedInterest.EarnedInterest.WorkingMode.AccountingLoanBalance)
				? oFirstIncomeDay
				: (
					(oDateStart < oFirstIncomeDay) ? oFirstIncomeDay : oDateStart
				);

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

			if (days.Length == 0) {
				LogAllDetails(
					0,
					"no dates found when the loan produced interest during report period",
					days,
					ifp,
					bp,
					bVerboseLogging,
					bAccountingMode,
					oWriteOffDate
				);
				return 0;
			} // if

			if (nMode == Reports.EarnedInterest.EarnedInterest.WorkingMode.AccountingLoanBalance) {
				if (days[days.Length - 1].Date < oDateStart) {
					LogAllDetails(
						0,
						"the loan has been completed before the report period",
						days,
						ifp,
						bp,
						bVerboseLogging,
						bAccountingMode,
						oWriteOffDate
					);
					return 0;
				} // if loan got repaid before report period
			} // if ForAudit mode

			InterestData[] aryDates = Schedule.Values.ToArray();

			int nDayDataPtr = 0;
			InterestData oCurDayData = aryDates[nDayDataPtr];

			foreach (PrInterest pri in days) {
				if (pri.Update(oCurDayData, ifp, bp, bAccountingMode, oWriteOffDate)) {
					nDayDataPtr++;

					if (nDayDataPtr < aryDates.Length)
						oCurDayData = aryDates[nDayDataPtr];
				} // if
			} // for

			decimal nEarnedInterest = days.Sum(pri => pri.Principal * pri.Interest);

			LogAllDetails(
				nEarnedInterest,
				"full details are below",
				days,
				ifp,
				bp,
				bVerboseLogging,
				bAccountingMode,
				oWriteOffDate
			);

			return nEarnedInterest;
		} // Calculate

		private string IfpToString(InterestFreezePeriods ifp) {
			string sIfp = "none";

			if (ifp != null) {
				var os = new StringBuilder();

				ifp.ForEach(i => os.AppendFormat("\n\t{0}", i));

				sIfp = os.ToString().Trim();
			} // if

			return sIfp;
		} // IfpToString

		private void LogAllDetails(
			decimal nEarnedInterest,
			string sMsg,
			IEnumerable<PrInterest> days,
			InterestFreezePeriods ifp,
			BadPeriods bp,
			bool bVerboseLogging,
			bool bAccountingMode,
			DateTime? oWriteOffDate
		) {
			if (!bVerboseLogging)
				return;

			if (string.IsNullOrWhiteSpace(sMsg))
				sMsg = string.Empty;
			else
				sMsg = " - " + sMsg;

			string sDaysList = (nEarnedInterest == 0) ? "none" : string.Join("\n\t", days.Select(pri => pri.ToString()).ToArray());

			m_oLog.Debug(
				"\n\nLoanID: {0} for customer {14}, {1} issued on {2} earned interest is {6}{11}.\n" +
				"Working mode: {15}\n" +
				"Write off date: {16}\n" +
				"Schedule ({7}):\n" +
				"\t{3}\n" +
				"Bad periods ({12}):\n" +
				"\t{13}\n" +
				"Freeze periods ({10}):\n" +
				"\t{9}\n" +
				"Transactions ({8}):\n" +
				"\t{4}\n" +
				"Per day:\n" +
				"\t{5}\n\n",
				m_nLoanID, Amount, IssueDate,
				string.Join("\n\t", Schedule.Values.Select(v => v.ToString()).ToArray()),
				string.Join("\n\t", Repayments.Values.Select(v => v.ToString()).ToArray()),
				sDaysList,
				nEarnedInterest,
				Schedule.Count,
				Repayments.Count,
				IfpToString(ifp),
				ifp == null ? 0 : ifp.Count,
				sMsg,
				bp == null ? 0 : bp.Count,
				bp == null ? "none" : bp.ToString(),
				CustomerID,
				bAccountingMode
					? "accounting mode (no interest earned after the first Write Off, other bad statuses are ignored)"
					: "normal mode (no interest earned during bad periods, Write Off is considered as just another bad status)",
				oWriteOffDate.HasValue ? oWriteOffDate.Value.ToString("MMM d yyyy", CultureInfo.InvariantCulture) : "none"
			);
		} // LogAllDetails

		private readonly int m_nLoanID;
		private readonly ASafeLog m_oLog;

	} // LoanData
} // namespace Reports

