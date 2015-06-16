namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class ExperianConsumerDataExt {
		public static int FindNumOfPersonalDefaults(
			this ExperianConsumerData oData,
			decimal minDefaultBalance,
			DateTime defaultAccountMinDate
		) {
			if ((oData == null) || (oData.Cais == null) || (oData.Cais.Count < 1))
				return 0;

			return FindNumOfPersonalDefaults(
				oData.Cais.Select(cais => new ExperianConsumerDataCaisAccounts {
					AccountStatusCodes = cais.AccountStatusCodes,
					Balance = cais.Balance,
					CurrentDefBalance = cais.CurrentDefBalance,
					LastUpdatedDate = cais.LastUpdatedDate,
					MatchTo = cais.MatchTo,
					Id = cais.Id
				}),
				minDefaultBalance,
				defaultAccountMinDate
			);
		} // FindNumOfPersonalDefaults

		public static int FindNumOfPersonalDefaults(
			IEnumerable<ExperianConsumerDataCaisAccounts> rawList,
			decimal minDefaultBalance,
			DateTime defaultAccountMinDate
		) {
			if (rawList == null)
				return 0;

			int numOfDefaultConsumerAccounts = 0;

			foreach (ExperianConsumerDataCaisAccounts cais in rawList)
				if (cais.IsPersonalDefault(minDefaultBalance, defaultAccountMinDate))
						numOfDefaultConsumerAccounts++;

			return numOfDefaultConsumerAccounts;
		} // FindNumOfPersonalDefaults
	} // class ExperianConsumerDataExt

	public static class ExperianConsumerDataCaisAccountsExt {
		public static bool IsPersonalDefault(
			this ExperianConsumerDataCaisAccounts cais,
			decimal minDefaultBalance,
			DateTime defaultAccountMinDate
		) {
			if (cais == null)
				return false;

			decimal nBalance = Math.Max(cais.CurrentDefBalance ?? 0, cais.Balance ?? 0);

			bool isRelevant = 
				(nBalance > minDefaultBalance) &&
				(cais.MatchTo == 1) &&
				cais.LastUpdatedDate.HasValue &&
				!string.IsNullOrWhiteSpace(cais.AccountStatusCodes);

			if (!isRelevant)
				return false;

			DateTime cur = cais.LastUpdatedDate.Value;

			for (int i = 1; i <= cais.AccountStatusCodes.Length; i++) {
				if (cur < defaultAccountMinDate)
					break;

				char status = cais.AccountStatusCodes[cais.AccountStatusCodes.Length - i];

				if ((status == '8') || (status == '9'))
					return true;

				cur = cur.AddMonths(-1);
			} // for

			return false;
		} // IsPersonalDefault
	} // class ExperianConsumerDataCaisAccountsExt
} // namespace
