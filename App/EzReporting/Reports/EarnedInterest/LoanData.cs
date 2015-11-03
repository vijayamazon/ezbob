namespace Reports.EarnedInterest {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Ezbob.Logger;

	internal class LoanData {
		public LoanData(int nLoanID, ASafeLog oLog) {
			this.loanID = nLoanID;
			this.log = oLog.Safe();

			this.Schedule = new SortedDictionary<DateTime, InterestData>();
			this.Repayments = new SortedDictionary<DateTime, TransactionData>();
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
			DateTime oFirstIncomeDay = this.IssueDate.AddDays(1);

			// A loan starts to produce interest on the next day.

			DateTime oDayOne = (oDateStart < oFirstIncomeDay) ? oFirstIncomeDay : oDateStart;

			// List of all the dates during requested period when a loan could produce interest.
			var oDaysList = new List<PrInterest>();

			for (DateTime d = oDayOne; d < oDateEnd; d = d.AddDays(1))
				oDaysList.Add(new PrInterest(d, this.Amount));

			if (oDaysList.Count == 0)
				return 0;

			DateTime oPrevDate = this.IssueDate;

			foreach (InterestData ida in this.Schedule.Values) {
				ida.PeriodLength = (ida.Date - oPrevDate).Days;
				oPrevDate = ida.Date;
			} // for each schedule

			oDaysList.ForEach(pri => {
				foreach (TransactionData t in this.Repayments.Values)
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

			InterestData[] aryDates = this.Schedule.Values.ToArray();

			foreach (PrInterest pri in days) {
				InterestData oCurDayData = pri.Date > aryDates[aryDates.Length - 1].Date
					? aryDates[aryDates.Length - 1]
					: aryDates.First(cdd => pri.Date <= cdd.Date);

				pri.Update(oCurDayData, ifp, bp, bAccountingMode, oWriteOffDate);
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

		public readonly SortedDictionary<DateTime, InterestData> Schedule;
		public readonly SortedDictionary<DateTime, TransactionData> Repayments;
		public DateTime IssueDate;
		public decimal Amount;
		public int CustomerID;

		private static string IfpToString(InterestFreezePeriods ifp) {
			string sIfp = "none";

			if (ifp == null)
				return sIfp;

			var os = new StringBuilder();

			ifp.ForEach(i => os.AppendFormat("\n\t{0}", i));

			sIfp = os.ToString().Trim();

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

			string sDaysList = (nEarnedInterest == 0)
				? "none"
				: string.Join("\n\t", days.Select(pri => pri.ToString()).ToArray());

			string accountingModeName = bAccountingMode
				? "accounting mode (no interest earned after the first Write Off, other bad statuses are ignored)"
				: "normal mode (no interest earned during bad periods, Write Off is considered as just another bad status)";

			this.log.Debug(
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
				this.loanID,
				this.Amount,
				this.IssueDate,
				string.Join("\n\t", this.Schedule.Values.Select(v => v.ToString()).ToArray()),
				string.Join("\n\t", this.Repayments.Values.Select(v => v.ToString()).ToArray()),
				sDaysList,
				nEarnedInterest,
				this.Schedule.Count,
				this.Repayments.Count,
				IfpToString(ifp),
				ifp == null ? 0 : ifp.Count,
				sMsg,
				bp == null ? 0 : bp.Count,
				bp == null ? "none" : bp.ToString(), this.CustomerID,
				accountingModeName,
				oWriteOffDate.HasValue ? oWriteOffDate.Value.ToString("MMM d yyyy", CultureInfo.InvariantCulture) : "none"
			);
		} // LogAllDetails

		private readonly int loanID;
		private readonly ASafeLog log;
	} // LoanData
} // namespace Reports
