namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class GetCompanyCaisDataForAlerts : AStrategy {
		private readonly int customerId;

		public GetCompanyCaisDataForAlerts(int customerId) {
			this.customerId = customerId;
		}
		public override string Name {
			get { return "GetCompanyCaisDataForAlerts"; }
		}

		public List<CompanyCaisAccount> Accounts { get; set; }
		public int NumOfCurrentDefaultAccounts { get; set; }
		public int NumOfSettledDefaultAccounts { get; set; }

		public override void Execute() {
			Accounts = new List<CompanyCaisAccount>();

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					DateTime lastUpdate = sr["CAISLastUpdatedDate"];
					string statuses = sr["AccountStatusLast12AccountStatuses"];

					Accounts.Add(new CompanyCaisAccount { LastUpdateDate = lastUpdate, Statuses = statuses });
					return ActionResult.Continue;
				},
				"GetCompanyCaisAccountsDataForAlerts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			SafeReader defaultsReader = DB.GetFirst(
				"GetCompanyCaisDefaultDataForAlerts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (!defaultsReader.IsEmpty) {
				NumOfCurrentDefaultAccounts = defaultsReader["NumOfCurrentDefaults"];
				NumOfSettledDefaultAccounts = defaultsReader["NumOfSettledDefaults"];
			}
		}
	}
}
