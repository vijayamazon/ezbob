namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillLandRegistry2PropertyLink : AStrategy
	{
		public BackfillLandRegistry2PropertyLink(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name { get { return "BackfillLandRegistry2PropertyLink"; } }

		public override void Execute()
		{
			var sh = new StrategyHelper();
			var linkedCustomers = new List<int>();
			DataTable entriesDataTable = DB.ExecuteReader("GetLandRegistryDataForBackfill", CommandSpecies.StoredProcedure);

			foreach (DataRow row in entriesDataTable.Rows)
			{
				try
				{
					var entriesSafeReader = new SafeReader(row);
					int customerId = entriesSafeReader["CustomerId"];
					string xml = entriesSafeReader["Response"];
					string titleNumber = entriesSafeReader["TitleNumber"];
					int landRegistryId = entriesSafeReader["Id"];

					if (linkedCustomers.Contains(customerId))
					{
						// Already found link for this customer
						continue;
					}

					sh.LinkLandRegistryAndAddress(customerId, xml, titleNumber, landRegistryId);
				}
				catch (Exception e)
				{
					Log.Error("Exception during backfill of land registry mapping", e);
				}
			}
		}
	}
}
