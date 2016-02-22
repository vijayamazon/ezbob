namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Backend.Strategies.UserManagement;
	using Ezbob.Database;

	public class BrokerUpdateEmail : AStrategy {
		public BrokerUpdateEmail(int changedByUserID, int brokerID, string newEmail) {
			this.changedByUserID = changedByUserID;
			this.brokerID = brokerID;
			this.newEmail = (newEmail ?? string.Empty).Trim().ToLowerInvariant();
			Result = string.Empty;
		} // constructor

		public override string Name {
			get { return "Broker update email"; }
		} // Name

		public string Result { get; private set; }

		public override void Execute() {
			Log.Debug("Updating password for broker {0} to '{1}...", this.brokerID, this.newEmail);

			if (string.IsNullOrWhiteSpace(this.newEmail))
				Result = "no email address provided";

			var brokerData = (new BrokerData(this, this.brokerID, DB));
			brokerData.Load();

			if (string.IsNullOrWhiteSpace(Result)) {
				Result = DB.ExecuteScalar<string>(
					"BrokerUpdateEmail",
					CommandSpecies.StoredProcedure,
					new QueryParameter("ChangedByUserID", this.changedByUserID),
					new QueryParameter("BrokerID", this.brokerID),
					new QueryParameter("NewEmail", this.newEmail),
					new QueryParameter("Now", DateTime.UtcNow)
				);
			} // if

			if (string.IsNullOrWhiteSpace(Result)) {
				new ResetPassword123456(this.brokerID, PasswordResetTarget.Broker).Execute();
				FireToBackground(new ChangeEmail(brokerData.Id, this.newEmail, brokerData.Email, brokerData.Origin));
			}

			Log.Debug(
				"Updating password for broker {0} to '{1}' completed {2}.",
				this.brokerID,
				this.newEmail,
				string.IsNullOrWhiteSpace(Result) ? "successfully" : "with error: " + Result
			);
		} // Execute

		private readonly int changedByUserID;
		private readonly int brokerID;
		private readonly string newEmail;
	} // class BrokerUpdateEmail
} // namespace
