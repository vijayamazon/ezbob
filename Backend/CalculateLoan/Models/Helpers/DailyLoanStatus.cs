namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Collections.Generic;
	using System.Linq;

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
			int dateLen = 0;
			int openPrincipalLen = 0;
			int dailyInterestLen = 0;
			int assignedFeeLen = 0;
			int dailyInterestRateLen = 0;
			int repaidPrincipalLen = 0;
			int repaidInterestLen = 0;
			int repaidFeeLen = 0;
			int notesLen = 0;

			const string date = "Date";
			const string openPrincipal = "Open principal";
			const string dailyInterest = "Daily accured interest";
			const string assignedFees = "Assigned fees";
			const string dailyInterestRate = "Daily interest rate";
			const string repaidPrincipal = "Principal";
			const string repaidInterest = "Interest";
			const string repaidFees = "Fees";
			const string notesTitle = "Notes";

			const string separator = "|";

			foreach (OneDayLoanStatus dd in Days) {
				dateLen = Math.Max(dateLen, dd.Str.Date.Length);
				openPrincipalLen = Math.Max(openPrincipalLen, dd.Str.OpenPrincipal.Length);
				dailyInterestLen = Math.Max(dailyInterestLen, dd.Str.DailyInterest.Length);
				assignedFeeLen = Math.Max(assignedFeeLen, dd.Str.AssignedFees.Length);
				dailyInterestRateLen = Math.Max(dailyInterestRateLen, dd.Str.DailyInterestRate.Length);
				repaidPrincipalLen = Math.Max(repaidPrincipalLen, dd.Str.RepaidPrincipal.Length);
				repaidInterestLen = Math.Max(repaidInterestLen, dd.Str.RepaidInterest.Length);
				repaidFeeLen = Math.Max(repaidFeeLen, dd.Str.RepaidFees.Length);
				notesLen = Math.Max(notesLen, GetNote(dd.Date).Length);
			} // for each day

			dateLen = Math.Max(dateLen, date.Length);
			openPrincipalLen = Math.Max(openPrincipalLen, openPrincipal.Length);
			dailyInterestLen = Math.Max(dailyInterestLen, dailyInterest.Length);
			assignedFeeLen = Math.Max(assignedFeeLen, assignedFees.Length);
			dailyInterestRateLen = Math.Max(dailyInterestRateLen, dailyInterestRate.Length);
			repaidPrincipalLen = Math.Max(repaidPrincipalLen, repaidPrincipal.Length);
			repaidInterestLen = Math.Max(repaidInterestLen, repaidInterest.Length);
			repaidFeeLen = Math.Max(repaidFeeLen, repaidFees.Length);
			notesLen = Math.Max(notesLen, notesTitle.Length);

			var repaidLen = repaidPrincipalLen + repaidInterestLen + repaidFeeLen + 4 + 2 * separator.Length;

			var result = new List<string>();

			result.Add(string.Format("{0}{1}{2}{1}", rowPrefix, separator, string.Join(
				separator,
				OneDayLoanStatus.FormatField(string.Empty, dateLen),
				OneDayLoanStatus.FormatField(string.Empty, openPrincipalLen),
				OneDayLoanStatus.FormatField(string.Empty, dailyInterestLen),
				OneDayLoanStatus.FormatField(string.Empty, assignedFeeLen),
				OneDayLoanStatus.FormatField(string.Empty, dailyInterestRateLen),
				OneDayLoanStatus.FormatField("Repaid", -repaidLen),
				OneDayLoanStatus.FormatField(string.Empty, notesLen)
			)));

			result.Add(string.Format("{0}{1}{2}{1}", rowPrefix, separator, string.Join(
				separator,
				OneDayLoanStatus.FormatField(date, -dateLen),
				OneDayLoanStatus.FormatField(openPrincipal, -openPrincipalLen),
				OneDayLoanStatus.FormatField(dailyInterest, -dailyInterestLen),
				OneDayLoanStatus.FormatField(assignedFees, -assignedFeeLen),
				OneDayLoanStatus.FormatField(dailyInterestRate, -dailyInterestRateLen),
				OneDayLoanStatus.FormatField(repaidPrincipal, -repaidPrincipalLen),
				OneDayLoanStatus.FormatField(repaidInterest, -repaidInterestLen),
				OneDayLoanStatus.FormatField(repaidFees, -repaidFeeLen),
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
					repaidPrincipalLen,
					repaidInterestLen,
					repaidFeeLen,
					-notesLen
				));
			} // for each day

			return string.Join("\n", result);
		} // ToFormattedString

		public void AddScheduleNotes(LoanCalculatorModel workingModel) {
			if (workingModel == null)
				return;

			for(int i = 0; i < workingModel.Schedule.Count; i++) {
				var sp = workingModel.Schedule[i];

				AddNote(
					sp.Date,
					i == workingModel.Schedule.Count - 1 ? "Last scheduled payment." : "Scheduled payment."
				);
			} // for each
		} // AddScheduleNotes

		public void AddPaymentNotes(LoanCalculatorModel workingModel) {
			if (workingModel == null)
				return;

			for(int i = 0; i < workingModel.Repayments.Count; i++) {
				var rp = workingModel.Repayments[i];

				AddNote(
					rp.Date,
					"Repaid: " + rp.Amount.ToString("C2", Library.Instance.Culture)
				);
			} // for each
		} // AddPaymentNotes

		public void AddFeeNotes(LoanCalculatorModel workingModel) {
			if (workingModel == null)
				return;

			for(int i = 0; i < workingModel.Fees.Count; i++) {
				var fee = workingModel.Fees[i];

				AddNote(
					fee.AssignDate,
					"Fee assigned: " + fee.Amount.ToString("C2", Library.Instance.Culture)
				);
			} // for each
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
