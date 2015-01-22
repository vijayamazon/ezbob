namespace Reports.EarnedInterest {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.ValueIntervals;

	public class EarnedInterest : SafeLog {
		public enum WorkingMode {
			/// <summary>
			///     Calculates earned interest for specified period
			///     over all the loans.
			/// </summary>
			ForPeriod,

			/// <summary>
			///     Calculates earned interest for loans that were issued
			///     during specified period. Earned interest is calculated
			///     for the date range between the earliest issued loan
			///     and today.
			/// </summary>
			ByIssuedLoans,

			/// <summary>
			///     Calculates earned interest for all loans that were issued
			///     to customers that are currently (i.e. at run time) are
			///     marked as CCI. Earned interest is calculated
			///     for the date range between the earliest issued loan
			///     and today.
			/// </summary>
			CciCustomers,
		} // enum WorkingMode

		public bool VerboseLogging { get; set; }

		public CustomerStatusHistory CustomerStatusHistory { get; private set; }

		public EarnedInterest(
			AConnection oDB,
			WorkingMode nMode,
			bool bAccountingMode,
			DateTime oDateOne,
			DateTime oDateTwo,
			ASafeLog oLog = null
			)
			: base(oLog) {
			VerboseLogging = false;

			this.m_oDB = oDB;

			if (oDateTwo < oDateOne) {
				DateTime tmp = oDateOne;
				oDateOne = oDateTwo;
				oDateTwo = tmp;
			} // if

			this.m_oDateStart = oDateOne;
			this.m_oDateEnd = oDateTwo;

			this.m_oLoans = new SortedDictionary<int, LoanData>();
			this.m_oFreezePeriods = new SortedDictionary<int, InterestFreezePeriods>();
			this.m_oBadPeriods = new SortedDictionary<int, BadPeriods>();

			this.m_nMode = nMode;

			this.m_bAccountingMode = bAccountingMode;
		} // constructor

		public SortedDictionary<int, decimal> Run() {
			switch (this.m_nMode) {
			case WorkingMode.ByIssuedLoans:
				FillBySp("RptEarnedInterest_IssuedLoans", false, false);
				break;

			case WorkingMode.ForPeriod:
				FillForPeriod();
				break;

			case WorkingMode.CciCustomers:
				FillBySp("RptEarnedInterest_CciCustomers", false, false);
				break;

			default:
				throw new ArgumentOutOfRangeException("Unsupported working mode: " + this.m_nMode, (Exception)null);
			} // switch

			FillFreezeIntervals();
			FillCustomerStatuses();

			return ProcessLoans();
		} // Run

		private void FillFreezeIntervals() {
			this.m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr["LoanId"];
					DateTime? oStart = sr["StartDate"];
					DateTime? oEnd = sr["EndDate"];
					decimal nRate = sr["InterestRate"];
					DateTime? oDeactivation = sr["DeactivationDate"];

					DateTime? oTo = oDeactivation.HasValue
						? (oEnd.HasValue ? DateInterval.Min(oEnd.Value, oDeactivation.Value) : oDeactivation)
						: oEnd;

					if (!this.m_oFreezePeriods.ContainsKey(nLoanID))
						this.m_oFreezePeriods[nLoanID] = new InterestFreezePeriods();

					this.m_oFreezePeriods[nLoanID].Add(oStart, oTo, nRate);

					return ActionResult.Continue;
				},
				"RptEarnedInterest_Freeze",
				CommandSpecies.StoredProcedure
				);
		} // FillFreezeIntervals

		private void FillCustomerStatuses() {
			CustomerStatusHistory = new CustomerStatusHistory(null, this.m_oDateEnd, this.m_oDB);

			foreach (KeyValuePair<int, List<CustomerStatusChange>> pair in CustomerStatusHistory.FullData.Data) {
				int nCustomerID = pair.Key;

				foreach (CustomerStatusChange csc in pair.Value) {
					try {
						bool bAlreadyHas = this.m_oBadPeriods.ContainsKey(nCustomerID);
						bool bLastKnown = !bAlreadyHas || this.m_oBadPeriods[nCustomerID].IsLastKnownGood;
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
								this.m_oBadPeriods[nCustomerID].Add(csc.ChangeDate, !bIsNewGood);
							else
								this.m_oBadPeriods[nCustomerID] = new BadPeriods(csc.ChangeDate);
						} // if
					} catch (Exception e) {
						Alert(e, "Failed to process customer status history entry.");
					} // try
				} // for each status change
			} // for each customer
		} // FillCustomerStatuses

		private SortedDictionary<int, decimal> ProcessLoans() {
			this.m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr[1];

					if (!this.m_oLoans.ContainsKey(nLoanID)) {
						if (VerboseLogging)
							Debug("Ignoring loan id {0}", nLoanID);

						return ActionResult.Continue;
					} // if

					DateTime oDate = sr[2];

					decimal nValue = sr[3];

					switch ((string)sr[0]) {
					case "0":
						this.m_oLoans[nLoanID].Schedule[oDate] = new InterestData(oDate, nValue);

						break;

					case "1":
						if (nValue > 0)
							this.m_oLoans[nLoanID].Repayments[oDate] = new TransactionData(oDate, nValue);

						break;
					} // switch

					return ActionResult.Continue;
				},
				"RptEarnedInterest_LoanDates",
				CommandSpecies.StoredProcedure
				);

			var oRes = new SortedDictionary<int, decimal>();

			foreach (KeyValuePair<int, LoanData> pair in this.m_oLoans) {
				InterestFreezePeriods ifp = this.m_oFreezePeriods.ContainsKey(pair.Key) ? this.m_oFreezePeriods[pair.Key] : null;

				BadPeriods bp = null;
				DateTime? oWriteOffDate = null;

				if (this.m_bAccountingMode)
					oWriteOffDate = CustomerStatusHistory.FullData.GetWriteOffDate(pair.Value.CustomerID);
				else
					bp = this.m_oBadPeriods.ContainsKey(pair.Value.CustomerID) ? this.m_oBadPeriods[pair.Value.CustomerID] : null;

				decimal nInterest = pair.Value.Calculate(this.m_oDateStart, this.m_oDateEnd,
					ifp,
					VerboseLogging, this.m_nMode, this.m_bAccountingMode,
					bp,
					oWriteOffDate
					);

				if (nInterest > 0)
					oRes[pair.Key] = nInterest;
			} // foreach

			return oRes;
		} // ProcessLoans

		private void FillForPeriod() {
			this.m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr[0];

					this.m_oLoans[nLoanID] = new LoanData(nLoanID, this) {
						IssueDate = sr[1],
						Amount = sr[2],
						CustomerID = sr[3],
					};

					return ActionResult.Continue;
				},
				"RptEarnedInterest_ForPeriod",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", this.m_oDateStart),
				new QueryParameter("@DateEnd", this.m_oDateEnd)
				);

			Info("{0} loans, date range: {1} - {2}", this.m_oLoans.Count, this.m_oDateStart, this.m_oDateEnd);
		} // FillForPeriod

		private void FillBySp(string sSpName, bool bKeepStartDate, bool bKeepEndDate) {
			DateTime oDateStart = DateTime.Now.AddYears(1980);

			if (!bKeepEndDate)
				this.m_oDateEnd = DateTime.Today.AddDays(1);

			this.m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr[0];
					DateTime oDate = sr[1];

					if (oDate < oDateStart)
						oDateStart = oDate;

					this.m_oLoans[nLoanID] = new LoanData(nLoanID, this) {
						IssueDate = oDate,
						Amount = sr[2],
						CustomerID = sr[3],
					};

					return ActionResult.Continue;
				},
				sSpName,
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", this.m_oDateStart),
				new QueryParameter("@DateEnd", this.m_oDateEnd)
				);

			if (!bKeepStartDate)
				this.m_oDateStart = oDateStart;

			Info("{0} loans, date range: {1} - {2}", this.m_oLoans.Count, this.m_oDateStart, this.m_oDateEnd);
		} // FillBySp

		private readonly SortedDictionary<int, LoanData> m_oLoans;
		private readonly SortedDictionary<int, InterestFreezePeriods> m_oFreezePeriods;
		private readonly SortedDictionary<int, BadPeriods> m_oBadPeriods;

		private readonly AConnection m_oDB;
		private readonly WorkingMode m_nMode;
		private readonly bool m_bAccountingMode;
		private DateTime m_oDateStart;
		private DateTime m_oDateEnd;
	} // class EarnedInterest
} // namespace
