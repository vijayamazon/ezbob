namespace Ezbob.Backend.CalculateLoan.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class DailyLoanStatus {
		public DailyLoanStatus() {
			Dates = new SortedDictionary<DateTime, OneDayLoanStatus>();
			this.notes = new SortedDictionary<DateTime, string>();
		} // constructor

		public void AddNote(DateTime dt, string note) {
			if (string.IsNullOrWhiteSpace(note))
				return;

			note = note.Trim();

			if (this.notes.ContainsKey(dt.Date))
				this.notes[dt.Date] += " " + note;
			else
				this.notes[dt.Date] = note;
		} // AddNote

		public OneDayLoanStatus LastDailyLoanStatus {
			get { return Days.LastOrDefault(); }
		} // LastDailyLoanStatus

		public void Add(OneDayLoanStatus odls) {
			if ((odls == null) || (this[odls.Date] != null))
				return;

			Dates[odls.Date] = odls;
		} // Add

		public bool Contains(DateTime dt) {
			return Dates.ContainsKey(dt.Date);
		} // Contains

		public IEnumerable<OneDayLoanStatus> Days {
			get { return Dates.Values; }
		} // Days

		public bool IsEmpty {
			get { return Dates.Count == 0; }
		} // IsEmpty

		public DateTime LastKnownDate {
			get { return IsEmpty ? DateTime.UtcNow.Date : Dates.Keys.Max(); }
		} // LastKnownDate

		public OneDayLoanStatus this[DateTime idx] {
			get { return Dates.ContainsKey(idx.Date) ? Dates[idx.Date] : null; }
		} // indexer

		public SortedDictionary<DateTime, OneDayLoanStatus> Dates { get; private set; }

		public IEnumerable<OneDayLoanStatus> Where(Func<OneDayLoanStatus, bool> filter) {
			return Days.Where(filter);
		} // Where

		public string GetNote(DateTime dd) {
			return this.notes.ContainsKey(dd.Date) ? this.notes[dd.Date] : string.Empty;
		} // GetNote

		public override string ToString() {
			return string.Join("\n\t\t", Days);
		} // ToString

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

			foreach (OneDayLoanStatus dd in Days) {
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
				OneDayLoanStatus.FormatField(string.Empty, dateLen),
				OneDayLoanStatus.FormatField("Open principal", -openPrincipalHeaderLen),
				OneDayLoanStatus.FormatField(string.Empty, dailyInterestLen),
				OneDayLoanStatus.FormatField(string.Empty, assignedFeeLen),
				OneDayLoanStatus.FormatField(string.Empty, dailyInterestRateLen),
				OneDayLoanStatus.FormatField(string.Empty, currentBalanceLen),
				OneDayLoanStatus.FormatField("Repaid", -repaidLen),
				OneDayLoanStatus.FormatField(string.Empty, ignoredDayLen),
				OneDayLoanStatus.FormatField(string.Empty, notesLen)
			)));

			result.Add(string.Format("{0}{1}{2}{1}", rowPrefix, separator, string.Join(
				separator,
				OneDayLoanStatus.FormatField(date, -dateLen),
				OneDayLoanStatus.FormatField(openPrincipalForInterest, -openPrincipalLen),
				OneDayLoanStatus.FormatField(openPrincipalAfterRepayments, -openPrincipalLen),
				OneDayLoanStatus.FormatField(dailyInterest, -dailyInterestLen),
				OneDayLoanStatus.FormatField(assignedFees, -assignedFeeLen),
				OneDayLoanStatus.FormatField(dailyInterestRate, -dailyInterestRateLen),
				OneDayLoanStatus.FormatField(currentBalance, -currentBalanceLen),
				OneDayLoanStatus.FormatField(repaidPrincipal, -repaidPrincipalLen),
				OneDayLoanStatus.FormatField(repaidInterest, -repaidInterestLen),
				OneDayLoanStatus.FormatField(repaidFees, -repaidFeeLen),
				OneDayLoanStatus.FormatField(ignoredDay, -ignoredDayLen),
				OneDayLoanStatus.FormatField(notesTitle, -notesLen)
			)));

			foreach (OneDayLoanStatus dd in Days) {
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

		public void AddScheduleNotes(NL_Model model) {
			if (model == null)
				return;

			List<NL_LoanSchedules> activeSchedule = new List<NL_LoanSchedules>();
			model.Loan.Histories.ForEach(h => activeSchedule.AddRange(h.ActiveSchedule()));
			int activeScheduleCount = activeSchedule.Count;
			int i = 0;

			foreach (NL_LoanSchedules s in activeSchedule) {
				AddNote(s.PlannedDate,i == activeScheduleCount - 1 ? "Last schedule." : "Schedule.");
				i++;
			}
		} // AddScheduleNotes

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
		} // AddPaymentNotes

		public void AddFeeNotes(NL_Model model) {
			if (model == null)
				return;
			
			foreach (NL_LoanFees fee in model.Loan.Fees) {
				AddNote(fee.AssignTime, "Fee assigned: " + fee.Amount.ToString("C2", Library.Instance.Culture));
			}

		} // AddFeeNotes

		/// <summary>
		/// Finite state machine with logic explanation: https://drive.google.com/open?id=0B1Io_qu9i44SYVZDMUVzX2NiVmc
		/// </summary>
		public void SetIgnoredDueToRescheduleDays() {
			if (IsEmpty)
				return;

			var days = new List<OneDayLoanStatus>(Dates.Values);

			foreach (OneDayLoanStatus day in days)
				day.IsBetweenLastPaymentAndReschedulingDay = false;

			int currentIdx = days.Count - 1;

			char state = 'A';

			for ( ; ; ) {
				if (currentIdx < 0)
					break;

				var current = days[currentIdx];

				switch (state) {
				case 'A':
					state = current.IsReschedulingDay ? 'C' : 'B';
					break;

				case 'B':
					currentIdx--;
					state = 'A';
					break;

				case 'C':
					currentIdx--;
					state = 'D';
					break;

				case 'D':
					if (current.IsReschedulingDay)
						state = 'C';
					else if (current.IsPaymentDay)
						state = 'B';
					else
						state = 'E';

					break;

				case 'E':
					current.IsBetweenLastPaymentAndReschedulingDay = true;
					state = 'C';
					break;
				} // switch
			} // forever
		} // SetIgnoredDueToRescheduleDays

		private readonly SortedDictionary<DateTime, string> notes;
	} // class DailyLoanStatus
} // namespace
