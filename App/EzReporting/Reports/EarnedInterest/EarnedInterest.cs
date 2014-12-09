namespace Reports.EarnedInterest {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.ValueIntervals;
	using Reports;

	public class EarnedInterest : SafeLog {

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

		public EarnedInterest(
			AConnection oDB,
			WorkingMode nMode,
			bool bAccountingMode,
			DateTime oDateOne,
			DateTime oDateTwo,
			ASafeLog oLog = null
		) : base(oLog) {
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
			m_oBadPeriods = new SortedDictionary<int, BadPeriods>();

			m_nMode = nMode;

			m_bAccountingMode = bAccountingMode;
		} // constructor

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
			FillCustomerStatuses();

			return ProcessLoans();
		} // Run

		public bool VerboseLogging { get; set; }

		public CustomerStatusHistory CustomerStatusHistory { get; private set; }

		private void FillFreezeIntervals() {
			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr["LoanId"];
					DateTime? oStart = sr["StartDate"];
					DateTime? oEnd = sr["EndDate"];
					decimal nRate = sr["InterestRate"];
					DateTime? oDeactivation = sr["DeactivationDate"];

					DateTime? oTo = oDeactivation.HasValue
						? (oEnd.HasValue ? DateInterval.Min(oEnd.Value, oDeactivation.Value) : oDeactivation)
						: oEnd;

					if (!m_oFreezePeriods.ContainsKey(nLoanID))
						m_oFreezePeriods[nLoanID] = new InterestFreezePeriods();

					m_oFreezePeriods[nLoanID].Add(oStart, oTo, nRate);

					return ActionResult.Continue;
				},
				"RptEarnedInterest_Freeze",
				CommandSpecies.StoredProcedure
			);
		} // FillFreezeIntervals

		private void FillCustomerStatuses() {
			CustomerStatusHistory = new CustomerStatusHistory(null, m_oDateEnd, m_oDB);

			foreach (KeyValuePair<int, List<CustomerStatusChange>> pair in CustomerStatusHistory.FullData.Data) {
				int nCustomerID = pair.Key;

				foreach (CustomerStatusChange csc in pair.Value) {
					try {
						bool bAlreadyHas = m_oBadPeriods.ContainsKey(nCustomerID);
						bool bLastKnown = !bAlreadyHas || m_oBadPeriods[nCustomerID].IsLastKnownGood;
						bool bIsOldGood = !BadPeriods.IsBad(csc.OldStatus);
						bool bIsNewGood = !BadPeriods.IsBad(csc.NewStatus);

						if (bLastKnown != bIsOldGood) {
							Alert(
								"Last known status is '{0}' while previous status is '{1}' for customer {2} on {3}.",
								(bLastKnown ? "good" : "bad"),
								(bIsOldGood ? "good" : "bad"),
								nCustomerID,
								csc.ChangeDate.ToString("MMM dd yyyy", CultureInfo.InvariantCulture)
							);
						} // if

						if (bLastKnown != bIsNewGood) {
							if (bAlreadyHas)
								m_oBadPeriods[nCustomerID].Add(csc.ChangeDate, !bIsNewGood);
							else
								m_oBadPeriods[nCustomerID] = new BadPeriods(csc.ChangeDate);
						} // if

					}
					catch (Exception e) {
						Alert(e, "Failed to process customer status history entry.");
					} // try
				} // for each status change
			} // for each customer
		} // FillCustomerStatuses

		private SortedDictionary<int, decimal> ProcessLoans() {
			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr[1];

					if (!m_oLoans.ContainsKey(nLoanID)) {
						if (VerboseLogging)
							Debug("Ignoring loan id {0}", nLoanID);

						return ActionResult.Continue;
					} // if

					DateTime oDate = sr[2];

					decimal nValue = sr[3];

					switch ((string)sr[0]) {
					case "0":
						m_oLoans[nLoanID].Schedule[oDate] = new InterestData(oDate, nValue);

						break;

					case "1":
						if (nValue > 0)
							m_oLoans[nLoanID].Repayments[oDate] = new TransactionData(oDate, nValue);

						break;
					} // switch

					return ActionResult.Continue;
				},
				"RptEarnedInterest_LoanDates",
				CommandSpecies.StoredProcedure
			);

			var oRes = new SortedDictionary<int, decimal>();

			foreach (KeyValuePair<int, LoanData> pair in m_oLoans) {
				InterestFreezePeriods ifp = m_oFreezePeriods.ContainsKey(pair.Key) ? m_oFreezePeriods[pair.Key] : null;

				BadPeriods bp = null;
				DateTime? oWriteOffDate = null;

				if (m_bAccountingMode)
					oWriteOffDate = CustomerStatusHistory.FullData.GetWriteOffDate(pair.Value.CustomerID);
				else
					bp = m_oBadPeriods.ContainsKey(pair.Value.CustomerID) ? m_oBadPeriods[pair.Value.CustomerID] : null;

				decimal nInterest = pair.Value.Calculate(
					m_oDateStart,
					m_oDateEnd,
					ifp,
					VerboseLogging,
					m_nMode,
					m_bAccountingMode,
					bp,
					oWriteOffDate
				);

				if (nInterest > 0)
					oRes[pair.Key] = nInterest;
			} // foreach

			return oRes;
		} // ProcessLoans

		private void FillForPeriod() {
			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr[0];

					m_oLoans[nLoanID] = new LoanData(nLoanID, this) {
						IssueDate = sr[1],
						Amount = sr[2],
						CustomerID = sr[3],
					};

					return ActionResult.Continue;
				},
				"RptEarnedInterest_ForPeriod", 
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", m_oDateStart),
				new QueryParameter("@DateEnd", m_oDateEnd)
			);

			Info("{0} loans, date range: {1} - {2}", m_oLoans.Count, m_oDateStart, m_oDateEnd);
		} // FillForPeriod

		private void FillBySp(string sSpName, bool bKeepStartDate, bool bKeepEndDate) {
			DateTime oDateStart = DateTime.Now.AddYears(1980);

			if (!bKeepEndDate)
				m_oDateEnd = DateTime.Today.AddDays(1);

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr[0];
					DateTime oDate = sr[1];

					if (oDate < oDateStart)
						oDateStart = oDate;

					m_oLoans[nLoanID] = new LoanData(nLoanID, this) {
						IssueDate = oDate,
						Amount = sr[2],
						CustomerID = sr[3],
					};

					return ActionResult.Continue;
				},
				sSpName,
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", m_oDateStart),
				new QueryParameter("@DateEnd", m_oDateEnd)
			);

			if (!bKeepStartDate)
				m_oDateStart = oDateStart;

			Info("{0} loans, date range: {1} - {2}", m_oLoans.Count, m_oDateStart, m_oDateEnd);
		} // FillBySp

		private readonly SortedDictionary<int, LoanData> m_oLoans;
		private readonly SortedDictionary<int, InterestFreezePeriods> m_oFreezePeriods;
		private readonly SortedDictionary<int, BadPeriods> m_oBadPeriods;

		private DateTime m_oDateStart;
		private DateTime m_oDateEnd;

		private readonly AConnection m_oDB;

		private readonly WorkingMode m_nMode;

		private readonly bool m_bAccountingMode;

	} // class EarnedInterest
} // namespace
