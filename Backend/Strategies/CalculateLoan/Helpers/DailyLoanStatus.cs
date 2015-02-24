namespace Ezbob.Backend.Strategies.CalculateLoan.Helpers {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class DailyLoanStatus {
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

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
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
			const string dailyInterest = "Daily accrued interest";
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

		private readonly SortedDictionary<DateTime, string> notes;
	} // class DailyLoanStatus
} // namespace
