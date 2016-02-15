namespace Ezbob.Backend.Strategies.SalesForce {
	using System;
	using Ezbob.Database;
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class ChangeEmail : AStrategy {
		private readonly int userID;
		private readonly string newEmail;
		private readonly string oldEmail;
		private readonly string origin;

		public ChangeEmail(int userID, string newEmail, string oldEmail, string origin) {
			this.userID = userID;
			this.newEmail = newEmail;
			this.oldEmail = oldEmail;
			this.origin = origin;
		} //ctor

		public override string Name { get { return "ChangeEmail"; } }

		public override void Execute() {
			ISalesForceAppClient salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();

			salesForce.ChangeEmail(new ChangeEmailModel { currentEmail = this.oldEmail, newEmail = this.newEmail, Origin = this.origin });

			if (salesForce.HasError) {
				DB.ExecuteNonQuery("SalesForceSaveError", CommandSpecies.StoredProcedure,
					new QueryParameter("Now", DateTime.UtcNow),
					new QueryParameter("CustomerID", this.userID),
					new QueryParameter("Type", this.Name),
					new QueryParameter("Model", salesForce.Model),
					new QueryParameter("Error", salesForce.Error));
			}//if
		}//Execute
	}//ChangeEmail
}//ns
