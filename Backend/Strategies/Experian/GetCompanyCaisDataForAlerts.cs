namespace EzBob.Backend.Strategies.Experian
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetCompanyCaisDataForAlerts : AStrategy
	{
		private readonly int customerId;

		public GetCompanyCaisDataForAlerts(AConnection oDb, ASafeLog oLog, int customerId)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}
		public override string Name
		{
			get { return "GetCompanyCaisDataForAlerts"; }
		}

		public List<CompanyCaisAccount> Accounts { get; set; }
		public int NumOfCurrentDefaultAccounts { get; set; }
		public int NumOfSettledDefaultAccounts { get; set; }

		public override void Execute()
		{
			Accounts = new List<CompanyCaisAccount>();
			DataTable accountsTable = DB.ExecuteReader(
				"GetCompanyCaisAccountsDataForAlerts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			foreach (DataRow row in accountsTable.Rows)
			{
				var sr = new SafeReader(row);

				DateTime lastUpdate = sr["CAISLastUpdatedDate"];
				string statuses = sr["AccountStatusLast12AccountStatuses"];

				Accounts.Add(new CompanyCaisAccount { LastUpdateDate = lastUpdate, Statuses = statuses });
			}

			DataTable defaultsTable = DB.ExecuteReader(
				"GetCompanyCaisDefaultDataForAlerts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			if (defaultsTable.Rows.Count == 1)
			{
				var sr = new SafeReader(defaultsTable.Rows[0]);
				NumOfCurrentDefaultAccounts = sr["NumOfCurrentDefaults"];
				NumOfSettledDefaultAccounts = sr["NumOfSettledDefaults"];
			}
		}
	}
}
