namespace Ezbob.Backend.Strategies.SalesForce {
	using Ezbob.Database;
	using SalesForceLib;
	using SalesForceLib.Models;

	public class AddUpdateContact : AStrategy {

		public AddUpdateContact(int customerID, int? directorID, string directorEmail) {
			salesForce = new ApiClient();
			this.customerID = customerID;
			this.directorID = directorID;
			this.directorEmail = directorEmail;
		}
		public override string Name { get { return "AddUpdateContact"; } }

		/// <summary>
		/// Retrieve and update sales force contact data.
		/// if director id or email is provided retrieve contact data for them, else for customer itself
		/// executed when directors are added in wizard/dashboard/UW or when customer/directors data is updated in dashboard
		/// </summary>
		public override void Execute() {
			ContactModel model = DB.FillFirst<ContactModel>("SF_LoadContact",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("DirectorID", directorID),
				new QueryParameter("DirectorEmail", directorEmail));

			salesForce.CreateUpdateContact(model);
		}
		private readonly ApiClient salesForce;
		private readonly int customerID;
		private readonly int? directorID;
		private readonly string directorEmail;
	}
}
