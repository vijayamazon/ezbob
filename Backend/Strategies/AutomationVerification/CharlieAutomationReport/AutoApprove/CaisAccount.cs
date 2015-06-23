namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.AutoApprove {
	using System;
	using System.Linq;
	using Ezbob.Backend.Extensions;
	using Ezbob.Database;

	internal class CaisAccount {
		[FieldName("Id")]
		public long ID { get; set; }

		public DateTime? LastUpdatedDate { get; set; }

		[FieldName("Balance")]
		public int? DBBalance { get; set; }

		[FieldName("CurrentDefBalance")]
		public int? DBCurrentDefBalance { get; set; }

		public string AccountStatusCodes { get; set; }

		public int Balance {
			get { return Math.Max(DBBalance ?? 0, DBCurrentDefBalance ?? 0); }
		} // Balance

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"ID {0}, updated on {1}, balance {2}, codes {3}",
				ID,
				(LastUpdatedDate ?? DateTime.UtcNow).DateStr(),
				Balance,
				AccountStatusCodes
			);
		} // ToString

		public bool IsBad(DateTime now) {
			if (LastUpdatedDate == null)
				return false;

			if (Balance < 500)
				return false;

			if (LastUpdatedDate.Value.Date > now.Date)
				return false;

			if (string.IsNullOrWhiteSpace(AccountStatusCodes))
				return false;

			DateTime thisMonth = MonthStart(now);
			DateTime lastUpdated = MonthStart(LastUpdatedDate.Value);

			// We do consider current month so we start 11 month ago (if today is June we start from the July last year).
			DateTime minConsideredDate = thisMonth.AddMonths(-11);

			if (lastUpdated < minConsideredDate)
				return false;

			int initialDistance = (thisMonth.Year == lastUpdated.Year)
				? thisMonth.Month - lastUpdated.Month
				: 12 - lastUpdated.Month + thisMonth.Month;

			//               0   1   2   3   4   5   6   7   8   9  10  11
			int[] codes = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, };

			DateTime cur = lastUpdated;

			for (int i = 1; i <= AccountStatusCodes.Length; i++) {
				if ((cur < minConsideredDate) || (cur > thisMonth))
					break;

				char status = AccountStatusCodes[AccountStatusCodes.Length - i];

				int distance = initialDistance - i + 1;

				if ((status >= '0') && (status != 'U'))
					codes[distance] = Math.Min(status - '0', 3); // If it's > 3 it doesn't really matter - to big anyway.
			} // for 12 months

			if (codes.Contains(3)) // There is a 90 days delay in last 12 months.
				return true;

			if (codes.Take(6).Contains(2)) // There is a 60 days delay in last 6 months.
				return true;

			if (codes.Take(3).Contains(1)) // There is a 30 days delay in last 3 months.
				return true;

			return false;
		} // IsBad

		private static DateTime MonthStart(DateTime d) {
			return new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		} // MonthStart
	} // class CaisAccount
} // namespace
