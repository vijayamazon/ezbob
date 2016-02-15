namespace Ezbob.Backend.Strategies.LandRegistry {
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;

	public class LandRegistryLoad : AStrategy {
		public LandRegistryLoad(int customerID) {
			this.customerID = customerID;
		} // constructor

		public override string Name { get { return "LandRegistryLoad"; } } // Name

		public List<LandRegistryDB> Result { get; set; }

		public override void Execute() {
			Result = DB.Fill<LandRegistryDB>("LandRegistryLoad", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", this.customerID));
			var lrIDs = Result.Select(x => x.Id);
			var owners = DB.Fill<LandRegistryOwnerDB>("LandRegistryOwnersLoad", CommandSpecies.StoredProcedure, Library.Instance.DB.CreateTableParameter("LandRegistryIDs", lrIDs));
			foreach (var owner in owners) {
				var lr = Result.FirstOrDefault(x => x.Id == owner.LandRegistryId);
				if (lr != null) {
					lr.Owners.Add(owner);
				}
			}//foreach
		} // Execute

		private readonly int customerID;
	} // class LandRegistryLoad
} // namespace
