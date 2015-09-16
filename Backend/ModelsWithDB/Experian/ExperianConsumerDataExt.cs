namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class ExperianConsumerDataExt {
		public static int FindNumOfPersonalDefaults(
			this ExperianConsumerData oData,
			decimal minDefaultBalance,
			DateTime defaultAccountMinDate,
			List<string> logList = null
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
				defaultAccountMinDate,
				logList
			);
		} // FindNumOfPersonalDefaults

		public static int FindNumOfPersonalDefaults(
			IEnumerable<ExperianConsumerDataCaisAccounts> rawList,
			decimal minDefaultBalance,
			DateTime defaultAccountMinDate,
			List<string> logList = null
		) {
			if (rawList == null)
				return 0;

			int numOfDefaultConsumerAccounts = 0;

			foreach (ExperianConsumerDataCaisAccounts cais in rawList)
				if (cais.IsPersonalDefault(minDefaultBalance, defaultAccountMinDate, logList))
						numOfDefaultConsumerAccounts++;

			return numOfDefaultConsumerAccounts;
		} // FindNumOfPersonalDefaults

		public static bool IsBad(this ExperianConsumerDataCais ca, DateTime now) {
			return ExperianConsumerDataCaisAccountsExt.IsBad(
				now,
				ca.LastUpdatedDate,
				Math.Max(ca.Balance ?? 0, ca.CurrentDefBalance ?? 0),
				ca.AccountStatusCodes
			);
		} // IsBad
	} // class ExperianConsumerDataExt
} // namespace
