namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Database;
	using Ezbob.Utils.Security;

	public class ChangeBrokerEmail : AStrategy {
		private readonly string oldEmail;
		private readonly string newEmail;
		private readonly string newPassword;

		public ChangeBrokerEmail(string oldEmail, string newEmail, string newPassword) {
			this.oldEmail = oldEmail;
			this.newEmail = newEmail;
			this.newPassword = newPassword;
		}

		public override string Name {
			get { return "ChangeBrokerEmail"; }
		}

		public override void Execute() {
			string hashedPassword = SecurityUtils.HashPassword(newEmail, newPassword);

			DB.ExecuteNonQuery("ChangeBrokerEmail", CommandSpecies.StoredProcedure,
				new QueryParameter("OldEmail", oldEmail),
				new QueryParameter("NewEmail", newEmail),
				new QueryParameter("NewPassword", hashedPassword));
		}
	}
}
