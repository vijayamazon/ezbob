namespace Ezbob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using EzBob.Models;
	using Ezbob.Database;

	public class BackfillLandRegistry2PropertyLink : AStrategy {
		public override string Name { get { return "BackfillLandRegistry2PropertyLink"; } }

		public override void Execute() {
			var sh = new StrategyHelper();
			var linkedCustomers = new List<int>();

			DB.ForEachRowSafe((entriesSafeReader, bRowsetStart) => {
				int customerId = entriesSafeReader["CustomerId"];
				string xml = entriesSafeReader["Response"];
				string titleNumber = entriesSafeReader["TitleNumber"];
				int landRegistryId = entriesSafeReader["Id"];

				if (linkedCustomers.Contains(customerId)) // Already found link for this customer
					return ActionResult.Continue;

				sh.LinkLandRegistryAndAddress(customerId, xml, titleNumber, landRegistryId);

				return ActionResult.Continue;
			}, "GetLandRegistryDataForBackfill", CommandSpecies.StoredProcedure);
		}
	}
}
