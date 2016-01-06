namespace Ezbob.Backend.Strategies.UserManagement {
	using System.Collections.Generic;
	using Ezbob.Database;

	public class LoadAllLoginRoles : AStrategy {
		public LoadAllLoginRoles(string login) {
			this.login = login;
			Roles = new List<string>();
		} // constructor

		public override string Name {
			get { return "LoadAllLoginRoles"; }
		} // Name

		public override void Execute() {
			Roles.Clear();
			DB.ForEachRowSafe(
				sr => Roles.Add(sr["Name"]),
				"LoadAllLoginRoles",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Login", this.login)
			);
		} // Execute

		public List<string> Roles { get; private set; }

		private readonly string login;
	} // class LoadAllLoginRoles
} // namespace

