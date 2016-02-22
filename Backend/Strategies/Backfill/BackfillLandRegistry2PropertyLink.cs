namespace Ezbob.Backend.Strategies.Backfill {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.LandRegistry;
	using Ezbob.Database;

	public class BackfillLandRegistry2PropertyLink : AStrategy {
		public override string Name { get { return "BackfillLandRegistry2PropertyLink"; } }

		public override void Execute() {
			var linkedCustomers = new SortedSet<int>();

			DB.ForEachRowSafe(entriesSafeReader => {
				int customerId = entriesSafeReader["CustomerId"];
				string xml = entriesSafeReader["Response"];
				string titleNumber = entriesSafeReader["TitleNumber"];
				int landRegistryId = entriesSafeReader["Id"];

				if (linkedCustomers.Contains(customerId)) // Already found link for this customer
					return;

				LandRegistryRes res = new LandRegistryRes(customerId, titleNumber);
				res.LinkLandRegistryAndAddress(customerId, xml, landRegistryId);
				linkedCustomers.Add(customerId);
			}, "GetLandRegistryDataForBackfill", CommandSpecies.StoredProcedure);
		}
	}
}
