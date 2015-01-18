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
				oData.Cais.Select(cais => (ExperianConsumerDataCaisAccounts)cais),
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

			IEnumerable<ExperianConsumerDataCaisAccounts> lst = rawList
				.Where(cais => {
					decimal nBalance = Math.Max(cais.CurrentDefBalance ?? 0, cais.Balance ?? 0);

					return
						(nBalance > minDefaultBalance) &&
						(cais.MatchTo == 1) &&
						cais.LastUpdatedDate.HasValue &&
						!string.IsNullOrWhiteSpace(cais.AccountStatusCodes);
				});

			// Log.Debug("Fill personal defaults: {0} found.", Grammar.Number(lst.Count, "relevant account"));

			// Log.Debug(
			// "Fill personal defaults: interested in data after {0}.",
			// defaultAccountMinDate.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
			// );

			foreach (ExperianConsumerDataCaisAccounts cais in lst) {
				// ReSharper disable PossibleInvalidOperationException
				// Log.Debug(
				// "Fill personal defaults cais {0}: updated on {1}, statuses is '{2}', now is {3}",
				// cais.Id,
				// cais.LastUpdatedDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				// cais.AccountStatusCodes,
				// Trail.InputData.DataAsOf.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				// );

				DateTime cur = cais.LastUpdatedDate.Value;
				// ReSharper restore PossibleInvalidOperationException

				for (int i = 1; i <= cais.AccountStatusCodes.Length; i++) {
					if (cur < defaultAccountMinDate) {
						// Log.Debug(
						// "Fill personal defaults cais {0} ain't no default: cur ({1}) is too early.",
						// cais.Id,
						// cur.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
						// );
						break;
					} // if

					char status = cais.AccountStatusCodes[cais.AccountStatusCodes.Length - i];

					// Log.Debug(
					// "Fill personal defaults cais {0} ain't no default: status[{1}] = '{2}' cur ({3}).",
					// cais.Id,
					// i,
					// status,
					// cur.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
					// );

					if ((status == '8') || (status == '9')) {
						// Log.Debug("Fill personal defaults cais {0} is default.", cais.Id);
						numOfDefaultConsumerAccounts++;
						break;
					} // if

					cur = cur.AddMonths(-1);
				} // for
			} // for each

			return numOfDefaultConsumerAccounts;
		} // FindNumOfPersonalDefaults
	} // class ExperianConsumerDataExt
} // namespace
