namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport {
	using System;
	using System.Linq;
	using Ezbob.Backend.Extensions;

	public static class CarCaisAccount {
		public static bool IsBad(DateTime now, DateTime? lastUpdatedDate, int balance, string accountStatusCodes) {
			if (lastUpdatedDate == null) {
				Library.Instance.Log.Debug("Not bad: last updated date is null.");
				return false;
			} // if

			if (balance < 500) {
				Library.Instance.Log.Debug("Not bad: balance {0} is less than 500.", balance);
				return false;
			} // if

			if (lastUpdatedDate.Value.Date > now.Date) {
				Library.Instance.Log.Debug(
					"Not bad: last updated date '{0}' > now '{1}'.",
					lastUpdatedDate.Value.MomentStr(),
					now.MomentStr()
				);
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(accountStatusCodes)) {
				Library.Instance.Log.Debug(
					"Not bad: account status codes '{0}' is null or white spaces.",
					accountStatusCodes
				);
				return false;
			} // if

			DateTime thisMonth = MonthStart(now);
			DateTime lastUpdated = MonthStart(lastUpdatedDate.Value);

			// We do consider current month so we start 11 month ago (if today is June we start from the July last year).
			DateTime minConsideredDate = thisMonth.AddMonths(-11);

			if (lastUpdated < minConsideredDate) {
				Library.Instance.Log.Debug(
					"Not bad: last updated '{0}' < min considered '{1}'.",
					lastUpdated.MomentStr(),
					minConsideredDate.MomentStr()
				);

				return false;
			} // if

			int initialDistance = (thisMonth.Year == lastUpdated.Year)
				? thisMonth.Month - lastUpdated.Month
				: 12 - lastUpdated.Month + thisMonth.Month;

			//               0   1   2   3   4   5   6   7   8   9  10  11
			int[] codes = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, };

			DateTime cur = lastUpdated;

			for (int i = 1; i <= accountStatusCodes.Length; i++) {
				if ((cur < minConsideredDate) || (cur > thisMonth))
					break;

				char status = accountStatusCodes[accountStatusCodes.Length - i];

				int distance = initialDistance + i - 1;

				Library.Instance.Log.Debug(
					"\n\tNow: "                + "'{0}'" +
					"\n\tThis month: "         + "'{1}'" +
					"\n\tMin considered: "     + "'{2}'" +
					"\n\tLast updated date: "  + "'{3}'" +
					"\n\tLast updated: "       + "'{4}'" +
					"\n\tCur: "                + "'{5}'" +
					"\n\tInitial distance: "   + "'{6}'" +
					"\n\ti: "                  + "'{7}'" +
					"\n\tDistance: "           + "'{8}'" +
					"\n\tCodes: "              + "'{9}'" +
					"\n\tStatus: "             + "'{10}' ({11})" +
					"\n\tBalance: "            + "'{12}'",
					now.MomentStr(),
					thisMonth.MomentStr(),
					minConsideredDate.MomentStr(),
					lastUpdatedDate.Value.MomentStr(),
					lastUpdated.MomentStr(),
					cur.MomentStr(),
					initialDistance,
					i,
					distance,
					accountStatusCodes,
					status,
					(int)status,
					balance
				);

				if ((status >= '0') && (status != 'U'))
					codes[distance] = Math.Min(status - '0', 3); // If it's > 3 it doesn't really matter - to big anyway.

				cur = cur.AddMonths(-1);
			} // for 12 months

			Library.Instance.Log.Debug("Last year: {0}.", string.Join(" ", codes));

			if (codes.Contains(3)) { // There is a 90 days delay in last 12 months.
				Library.Instance.Log.Debug("Bad: 90 days in last year.");
				return true;
			} // if

			if (codes.Take(6).Contains(2)) { // There is a 60 days delay in last 6 months.
				Library.Instance.Log.Debug("Bad: 60 days in last half year.");
				return true;
			} // 

			if (codes.Take(3).Contains(1)) { // There is a 30 days delay in last 3 months.
				Library.Instance.Log.Debug("Bad: 30 days in last quarter.");
				return true;
			} // if

			Library.Instance.Log.Debug("Not bad: nothing found.");

			return false;
		} // IsBad

		private static DateTime MonthStart(DateTime d) {
			return new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		} // MonthStart
	} // class CarCaisAccount
} // namespace
