namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport {
	using System;
	using System.Linq;
	using Ezbob.Backend.Extensions;

	public static class CarCaisAccount {
		public static bool IsBad(DateTime now, DateTime? lastUpdatedDate, int balance, string accountStatusCodes) {
			if (lastUpdatedDate == null)
				return false;

			if (balance < 500)
				return false;

			if (lastUpdatedDate.Value.Date > now.Date)
				return false;

			if (string.IsNullOrWhiteSpace(accountStatusCodes))
				return false;

			DateTime thisMonth = MonthStart(now);
			DateTime lastUpdated = MonthStart(lastUpdatedDate.Value);

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

			for (int i = 1; i <= accountStatusCodes.Length; i++) {
				if ((cur < minConsideredDate) || (cur > thisMonth))
					break;

				char status = accountStatusCodes[accountStatusCodes.Length - i];

				int distance = initialDistance + i - 1;

				/*
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
					"\n\tStatus: "             + "'{10}' ({11})",
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
					(int)status
				);
				*/

				if ((status >= '0') && (status != 'U'))
					codes[distance] = Math.Min(status - '0', 3); // If it's > 3 it doesn't really matter - to big anyway.

				cur = cur.AddMonths(-1);
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
	} // class CarCaisAccount
} // namespace
