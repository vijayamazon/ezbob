namespace Ezbob.Backend.Strategies.CalculateLoan.Helpers {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class DailyLoanStatus {
		public DailyLoanStatus() {
			Dates = new SortedDictionary<DateTime, OneDayLoanStatus>();
		} // constructor

		public void Add(OneDayLoanStatus odls) {
			if ((odls == null) || (this[odls.Date] != null))
				return;

			Dates[odls.Date] = odls;
		} // Add

		public IEnumerable<OneDayLoanStatus> Days {
			get { return Dates.Values; }
		} // Days

		public bool IsEmpty {
			get { return Dates.Count == 0; }
		} // IsEmpty

		public DateTime LastKnownDate {
			get { return IsEmpty ? DateTime.UtcNow : Dates.Keys.Max(); }
		} // LastKnownDate

		public OneDayLoanStatus this[DateTime idx] {
			get { return Dates.ContainsKey(idx.Date) ? Dates[idx.Date] : null; }
		} // indexer

		public SortedDictionary<DateTime, OneDayLoanStatus> Dates { get; private set; }

		public IEnumerable<OneDayLoanStatus> Where(Func<OneDayLoanStatus, bool> filter) {
			return Days.Where(filter);
		} // Where

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Join("\n\t\t", Days);
		} // ToString
	} // class DailyLoanStatus
} // namespace
