namespace Ezbob.Backend.Strategies.UserManagement {
	using System.Collections.Generic;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class LoadAllLoginRoles : AStrategy {
		public LoadAllLoginRoles(string login, CustomerOriginEnum? origin, bool ignoreOrigin) {
			this.login = login;
			this.origin = origin == null ? (int?)null : (int)origin.Value;
			this.ignoreOrigin = ignoreOrigin;
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
				new QueryParameter("@Login", this.login),
				new QueryParameter("@OriginID", this.origin),
				new QueryParameter("@IgnoreOrigin", this.ignoreOrigin)
			);
		} // Execute

		public List<string> Roles { get; private set; }

		private readonly string login;
		private readonly int? origin;
		private readonly bool ignoreOrigin;
	} // class LoadAllLoginRoles
} // namespace

