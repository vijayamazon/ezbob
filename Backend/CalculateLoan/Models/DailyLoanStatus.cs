namespace Ezbob.Backend.CalculateLoan.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class DailyLoanStatus {
		public DailyLoanStatus() {
			Dates = new SortedDictionary<DateTime, OneDayState>();
			this.notes = new SortedDictionary<DateTime, string>();
		} // constructor

		public SortedDictionary<DateTime, OneDayState> Dates { get; private set; }

		public IEnumerable<OneDayState> Days {
			get { return Dates.Values; }
		}

		public DateTime LastKnownDate {
			get { return IsEmpty ? DateTime.UtcNow.Date : Dates.Keys.Max(); }
		}

		public OneDayState LastDailyLoanStatus {
			get { return Days.LastOrDefault(); }
		}

		public void Add(OneDayState odls) {
			if ((odls == null) || (this[odls.Date] != null))
				return;

			Dates[odls.Date] = odls;
		}

		public bool Contains(DateTime dt) {
			return Dates.ContainsKey(dt.Date);
		}

		public bool IsEmpty {
			get { return Dates.Count == 0; }
		}

		public OneDayState this[DateTime idx] {
			get {
				OneDayState value;
				return Dates.TryGetValue(idx.Date, out value) ? value : null;
			}
		} // indexer

		public IEnumerable<OneDayState> Where(Func<OneDayState, bool> filter) {
			return Days.Where(filter);
		}

		public string GetNote(DateTime dd) {
			string value;
			return this.notes.TryGetValue(dd.Date, out value) ? value : string.Empty;
		}

		public void AddNote(DateTime dt, string note) {
			if (string.IsNullOrWhiteSpace(note))
				return;

			note = note.Trim();

			if (this.notes.ContainsKey(dt.Date))
				this.notes[dt.Date] += " " + note;
			else
				this.notes[dt.Date] = note;
		}

		public void AddScheduleNotes(NL_Model model) {
			if (model == null)
				return;

			List<NL_LoanSchedules> activeSchedule = new List<NL_LoanSchedules>();
			model.Loan.Histories.ForEach(h => activeSchedule.AddRange(h.ActiveSchedule()));
			int activeScheduleCount = activeSchedule.Count;
			int i = 0;

			foreach (NL_LoanSchedules s in activeSchedule) {
				AddNote(s.PlannedDate, i == activeScheduleCount - 1 ? "Last schedule." : "Schedule.");
				i++;
			}
		}

		public void AddPaymentNotes(NL_Model model) {
			if (model == null)
				return;

			//for(int i = 0; i < workingModel.Repayments.Count; i++) {
			//	var rp = workingModel.Repayments[i];

			//	AddNote(
			//		rp.Date,
			//		"Repaid: " + rp.Amount.ToString("C2", Library.Instance.Culture)
			//	);
			//} // for each
		}

		public void AddFeeNotes(NL_Model model) {
			if (model == null)
				return;

			foreach (NL_LoanFees fee in model.Loan.Fees) {
				AddNote(fee.AssignTime, "Fee assigned: " + fee.Amount.ToString("C2", Library.Instance.Culture));
			}
		}

		public override string ToString() {
			return string.Join("\n\t\t", Days);
		}

		public string ToFormattedString(string rowPrefix) {
			const string date = "Date";
			const string openPrincipalForInterest = "For interest";
			const string openPrincipalAfterRepayments = "After repayments";
			const string dailyInterest = "Daily accrued interest";
			const string assignedFees = "Assigned fees";
			const string dailyInterestRate = "Daily interest rate";
			const string currentBalance = "Current balance";
			const string repaidPrincipal = "Principal";
			const string repaidInterest = "Interest";
			const string repaidFees = "Fees";
			const string ignoredDay = "Ignored";
			const string notesTitle = "Notes";

			const string separator = "|";

			int dateLen = date.Length;
			int openPrincipalLen = Math.Max(openPrincipalForInterest.Length, openPrincipalAfterRepayments.Length);
			int dailyInterestLen = dailyInterest.Length;
			int assignedFeeLen = assignedFees.Length;
			int dailyInterestRateLen = dailyInterestRate.Length;
			int currentBalanceLen = currentBalance.Length;
			int repaidPrincipalLen = repaidPrincipal.Length;
			int repaidInterestLen = repaidInterest.Length;
			int repaidFeeLen = repaidFees.Length;
			int ignoredDayLen = ignoredDay.Length;
			int notesLen = notesTitle.Length;

			foreach (OneDayState dd in Days) {
				dateLen = Math.Max(dateLen, dd.Str.Date.Length);
				openPrincipalLen = Math.Max(openPrincipalLen, dd.Str.OpenPrincipalForInterest.Length);
				openPrincipalLen = Math.Max(openPrincipalLen, dd.Str.OpenPrincipalAfterRepayments.Length);
				dailyInterestLen = Math.Max(dailyInterestLen, dd.Str.DailyInterest.Length);
				assignedFeeLen = Math.Max(assignedFeeLen, dd.Str.AssignedFees.Length);
				dailyInterestRateLen = Math.Max(dailyInterestRateLen, dd.Str.DailyInterestRate.Length);
				currentBalanceLen = Math.Max(currentBalanceLen, dd.Str.CurrentBalance.Length);
				repaidPrincipalLen = Math.Max(repaidPrincipalLen, dd.Str.RepaidPrincipal.Length);
				repaidInterestLen = Math.Max(repaidInterestLen, dd.Str.RepaidInterest.Length);
				repaidFeeLen = Math.Max(repaidFeeLen, dd.Str.RepaidFees.Length);
				notesLen = Math.Max(notesLen, GetNote(dd.Date).Length);
			} // for each day

			var repaidLen = repaidPrincipalLen + repaidInterestLen + repaidFeeLen + 4 + 2 * separator.Length;

			var openPrincipalHeaderLen = 2 * openPrincipalLen + 1 + 2 * separator.Length;

			var result = new List<string>();

			result.Add(string.Format("{0}{1}{2}{1}", rowPrefix, separator, string.Join(
				separator,
				OneDayState.FormatField(string.Empty, dateLen),
				OneDayState.FormatField("Open principal", -openPrincipalHeaderLen),
				OneDayState.FormatField(string.Empty, dailyInterestLen),
				OneDayState.FormatField(string.Empty, assignedFeeLen),
				OneDayState.FormatField(string.Empty, dailyInterestRateLen),
				OneDayState.FormatField(string.Empty, currentBalanceLen),
				OneDayState.FormatField("Repaid", -repaidLen),
				OneDayState.FormatField(string.Empty, ignoredDayLen),
				OneDayState.FormatField(string.Empty, notesLen)
			)));

			result.Add(string.Format("{0}{1}{2}{1}", rowPrefix, separator, string.Join(
				separator,
				OneDayState.FormatField(date, -dateLen),
				OneDayState.FormatField(openPrincipalForInterest, -openPrincipalLen),
				OneDayState.FormatField(openPrincipalAfterRepayments, -openPrincipalLen),
				OneDayState.FormatField(dailyInterest, -dailyInterestLen),
				OneDayState.FormatField(assignedFees, -assignedFeeLen),
				OneDayState.FormatField(dailyInterestRate, -dailyInterestRateLen),
				OneDayState.FormatField(currentBalance, -currentBalanceLen),
				OneDayState.FormatField(repaidPrincipal, -repaidPrincipalLen),
				OneDayState.FormatField(repaidInterest, -repaidInterestLen),
				OneDayState.FormatField(repaidFees, -repaidFeeLen),
				OneDayState.FormatField(ignoredDay, -ignoredDayLen),
				OneDayState.FormatField(notesTitle, -notesLen)
			)));

			foreach (OneDayState dd in Days) {
				result.Add(dd.ToFormattedString(
					rowPrefix,
					separator,
					GetNote(dd.Date),
					-dateLen,
					openPrincipalLen,
					dailyInterestLen,
					assignedFeeLen,
					dailyInterestRateLen,
					currentBalanceLen,
					repaidPrincipalLen,
					repaidInterestLen,
					repaidFeeLen,
					ignoredDayLen,
					-notesLen
				));
			} // for each day

			return string.Join("\n", result);
		} // ToFormattedString



		/// <summary>
		/// Finite state machine with logic explanation: https://drive.google.com/open?id=0B1Io_qu9i44SYVZDMUVzX2NiVmc
		/// </summary>
		//public void SetIgnoredDueToRescheduleDays() {
		//	if (IsEmpty)
		//		return;

		//	var days = new List<OneDayState>(Dates.Values);

		//	foreach (OneDayState day in days)
		//		day.IsBetweenLastPaymentAndReschedulingDay = false;

		//	int currentIdx = days.Count - 1;

		//	char state = 'A';

		//	for ( ; ; ) {
		//		if (currentIdx < 0)
		//			break;

		//		var current = days[currentIdx];

		//		switch (state) {
		//		case 'A':
		//			state = current.IsReschedulingDay ? 'C' : 'B';
		//			break;

		//		case 'B':
		//			currentIdx--;
		//			state = 'A';
		//			break;

		//		case 'C':
		//			currentIdx--;
		//			state = 'D';
		//			break;

		//		case 'D':
		//			if (current.IsReschedulingDay)
		//				state = 'C';
		//			else if (current.IsPaymentDay)
		//				state = 'B';
		//			else
		//				state = 'E';

		//			break;

		//		case 'E':
		//			current.IsBetweenLastPaymentAndReschedulingDay = true;
		//			state = 'C';
		//			break;
		//		} // switch
		//	} // forever
		//} // SetIgnoredDueToRescheduleDays

		private readonly SortedDictionary<DateTime, string> notes;
	} // class DailyLoanStatus
} // namespace
