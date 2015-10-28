namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;

	public static class ExperianConsumerDataCaisAccountsExt {
		public static bool IsPersonalDefault(
			this ExperianConsumerDataCaisAccounts cais,
			decimal minDefaultBalance,
			DateTime defaultAccountMinDate,
			List<string> logList = null
		) {
			const string rowTag = "Personal default detector: ";

			if (cais == null) {
				LogToList(logList, "{0} not a default, data to check is null.", rowTag);
				return false;
			} // if

			decimal nBalance = Math.Max(cais.CurrentDefBalance ?? 0, cais.Balance ?? 0);

			bool isRelevant = 
				(nBalance > minDefaultBalance) &&
				(cais.MatchTo == 1) &&
				cais.LastUpdatedDate.HasValue &&
				!string.IsNullOrWhiteSpace(cais.AccountStatusCodes);

			LogToList(
				logList,
				"{0} CAIS id {1}: " +
				"balance = {2}, min balance = {3}; " +
				"match to = {4}; last updated = {5}; status codes = '{6}'.",
				rowTag,
				cais.Id,
				nBalance,
				minDefaultBalance,
				cais.MatchTo,
				cais.LastUpdatedDate.MomentStr(),
				cais.AccountStatusCodes
			);

			if (!isRelevant) {
				LogToList(logList, "{0} not a default, CAIS id {1} is not relevant.", rowTag, cais.Id);
				return false;
			} // if

			DateTime defaultAccountMinMonth = new DateTime(
				defaultAccountMinDate.Year,
				defaultAccountMinDate.Month,
				1,
				0,
				0,
				0,
				DateTimeKind.Utc
			).AddMonths(1);

			DateTime cur = new DateTime(
				cais.LastUpdatedDate.Value.Year,
				cais.LastUpdatedDate.Value.Month,
				1,
				0,
				0,
				0,
				DateTimeKind.Utc
			);

			LogToList(
				logList,
				"{0} CAIS id {1}, current date = {2}, default account min date = {3}, " +
				"default account min month = {4}, status code length = {5}.",
				rowTag,
				cais.Id,
				cur.MomentStr(),
				defaultAccountMinDate.MomentStr(),
				defaultAccountMinMonth.MomentStr(),
				cais.AccountStatusCodes.Length
			);

			for (int i = 1; i <= cais.AccountStatusCodes.Length; i++) {
				if (cur < defaultAccountMinMonth) {
					LogToList(
						logList,
						"{0} CAIS id {1}, current date = {2} is less than default account min date = {3}.",
						rowTag,
						cais.Id,
						cur.MomentStr(),
						defaultAccountMinMonth.MomentStr()
					);

					break;
				} // if

				char status = cais.AccountStatusCodes[cais.AccountStatusCodes.Length - i];

				LogToList(
					logList,
					"{0} CAIS id {1}, current date = {2}, status code length = {3}, i = {4}, status = {5}.",
					rowTag,
					cais.Id,
					cur.MomentStr(),
					cais.AccountStatusCodes.Length,
					i,
					status.ToString()
				);

				if ((status == '8') || (status == '9')) {
					LogToList(
						logList,
						"{0} CAIS id {1} is default, status = {2}.",
						rowTag,
						cais.Id,
						status.ToString()
					);

					return true;
				} // if

				cur = cur.AddMonths(-1);
			} // for

			LogToList(logList, "{0} not a default, CAIS id {1} ain't no contains default status.", rowTag, cais.Id);

			return false;
		} // IsPersonalDefault

		public static bool IsBad(
			DateTime now,
			DateTime? lastUpdatedDate,
			int balance,
			string accountStatusCodes,
			List<string> logList = null
		) {
			if (lastUpdatedDate == null) {
				LogToList(logList, "Not bad: last updated date is null.");
				return false;
			} // if

			if (balance < 500) {
				LogToList(logList, "Not bad: balance {0} is less than 500.", balance);
				return false;
			} // if

			if (lastUpdatedDate.Value.Date > now.Date) {
				LogToList(logList,
					"Not bad: last updated date '{0}' > now '{1}'.",
					lastUpdatedDate.Value.MomentStr(),
					now.MomentStr()
				);
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(accountStatusCodes)) {
				LogToList(logList,
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
				LogToList(logList,
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

				LogToList(logList,
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

			LogToList(logList, "Last year: {0}.", string.Join(" ", codes));

			if (codes.Contains(3)) { // There is a 90 days delay in last 12 months.
				LogToList(logList, "Bad: 90 days in last year.");
				return true;
			} // if

			if (codes.Take(6).Contains(2)) { // There is a 60 days delay in last 6 months.
				LogToList(logList, "Bad: 60 days in last half year.");
				return true;
			} // 

			if (codes.Take(3).Contains(1)) { // There is a 30 days delay in last 3 months.
				LogToList(logList, "Bad: 30 days in last quarter.");
				return true;
			} // if

			LogToList(logList, "Not bad: nothing found.");

			return false;
		} // IsBad

		private static DateTime MonthStart(DateTime d) {
			return new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		} // MonthStart

		private static void LogToList(List<string> logList, string format, params object[] args) {
			if (logList == null)
				return;

			logList.Add(string.Format(format, args));
		} // LogToList

		private static string MomentStr(this DateTime? dt) {
			return dt == null ? "NOT SPECIFIED" : dt.Value.ToString("MMM dd yyyy H:mm:ss", CultureInfo.InvariantCulture);
		} // MomentStr

		private static string MomentStr(this DateTime dt) {
			return dt.ToString("MMM dd yyyy H:mm:ss", CultureInfo.InvariantCulture);
		} // MomentStr
	} // class ExperianConsumerDataCaisAccountsExt
} // namespace
