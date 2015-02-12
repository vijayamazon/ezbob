namespace Ezbob.Backend.Strategies.SalesForce {
	using System.Threading;
	using Ezbob.Database;
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddUpdateContact : AStrategy {
		public AddUpdateContact(int customerID, int? directorID, string directorEmail) {
			salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
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
			Thread.Sleep(60000); // Solves race condition in converting lead to account on finish wizard, in order to improve sleep only when invoked from finish wizard flow
			ContactModel model = DB.FillFirst<ContactModel>("SF_LoadContact",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("DirectorID", directorID),
				new QueryParameter("DirectorEmail", directorEmail));

			salesForce.CreateUpdateContact(model);
		}
		private readonly ISalesForceAppClient salesForce;
		private readonly int customerID;
		private readonly int? directorID;
		private readonly string directorEmail;
	}
}
