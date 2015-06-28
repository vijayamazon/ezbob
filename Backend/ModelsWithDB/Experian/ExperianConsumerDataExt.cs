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
} // namespace
