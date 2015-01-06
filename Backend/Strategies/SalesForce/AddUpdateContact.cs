namespace Ezbob.Backend.Strategies.SalesForce {
	using Ezbob.Database;
	using SalesForceLib;
	using SalesForceLib.Models;

	public class AddUpdateContact : AStrategy {

		public AddUpdateContact(int customerID, int? directorID) {
			salesForce = new ApiClient();
			this.customerID = customerID;
			this.directorID = directorID;
		}
		public override string Name { get { return "AddUpdateContact"; } }

		public override void Execute() {
			ContactModel model = DB.FillFirst<ContactModel>("SF_LoadContact",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("DirectorID", directorID));

			salesForce.CreateUpdateContact(model);
		}
		private readonly ApiClient salesForce;
		private readonly int customerID;
		private readonly int? directorID;
	}
}
