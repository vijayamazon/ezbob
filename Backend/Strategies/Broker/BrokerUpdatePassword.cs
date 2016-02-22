namespace Ezbob.Backend.Strategies.Broker {
	using System.Data;
	using ConfigManager;
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using Ezbob.Utils.Security;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class BrokerUpdatePassword : AStrategy {
		public BrokerUpdatePassword(
			string sContactEmail,
			CustomerOriginEnum origin,
			DasKennwort oldPassword,
			DasKennwort newPassword,
			DasKennwort newPasswordAgain
		) {
			this.spLoad = new BrokerLoadDetailsForPasswordUpdate(DB, Log) {
				ContactEmail = sContactEmail,
				OriginID = (int)origin,
			};

			this.oldPassword = oldPassword.Decrypt();
			this.newPassword = newPassword.Decrypt();
			this.newPasswordAgain = newPasswordAgain.Decrypt();
		} // constructor

		public override string Name {
			get { return "Broker update password"; }
		} // Name

		public override void Execute() {
			bool emptyPassword =
				string.IsNullOrWhiteSpace(this.oldPassword) ||
				string.IsNullOrWhiteSpace(this.newPassword) ||
				string.IsNullOrWhiteSpace(this.newPasswordAgain);

			if (emptyPassword)
				throw new StrategyWarning(this, "Password not specified for broker '" + this.spLoad.ContactEmail + "'.");

			if (this.oldPassword == this.newPassword) {
				throw new StrategyWarning(
					this,
					"New and old passwords are the same for broker '" + this.spLoad.ContactEmail + "'."
				);
			} // if

			if (this.newPassword != this.newPasswordAgain) {
				throw new StrategyWarning(
					this,
					"New password and its confirmation are not the same for broker '" + this.spLoad.ContactEmail + "'."
				);
			} // if

			this.spLoad.ExecuteNonQuery();

			if (BrokerID < 1)
				throw new StrategyWarning(this, "Failed to find broker by email '" + this.spLoad.ContactEmail + "'.");

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			var currentHashed = new HashedPassword(
				this.spLoad.ContactEmail,
				this.spLoad.CycleCount,
				this.spLoad.EzPassword,
				this.spLoad.Salt
			);

			if (!pu.Validate(this.oldPassword, currentHashed)) {
				throw new StrategyWarning(
					this,
					"Current password does not match for broker by email '" + this.spLoad.ContactEmail + "'."
				);
			} // if

			var hashed = pu.Generate(this.spLoad.ContactEmail, this.newPassword);

			new SpBrokerUpdatePassword(BrokerID, hashed, DB, Log).ExecuteNonQuery();

			FireToBackground(new BrokerPasswordChanged(BrokerID, this.newPassword));
		} // Execute

		public int BrokerID { get { return this.spLoad.BrokerID; } }

		private readonly BrokerLoadDetailsForPasswordUpdate spLoad;

		private readonly string oldPassword;
		private readonly string newPassword;
		private readonly string newPasswordAgain;

		private class BrokerLoadDetailsForPasswordUpdate : AStoredProcedure {
			public BrokerLoadDetailsForPasswordUpdate(AConnection db, ASafeLog log) : base(db, log) {
				BrokerID = 0;
			} // constructor

			public override bool HasValidParameters() {
				return !string.IsNullOrWhiteSpace(ContactEmail) && (OriginID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public string ContactEmail { get; set; }

			[UsedImplicitly]
			public int OriginID { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public int BrokerID { get; set; }

			[UsedImplicitly]
			[Length(255)]
			[Direction(ParameterDirection.Output)]
			public string EzPassword { get; set; }

			[UsedImplicitly]
			[Length(255)]
			[Direction(ParameterDirection.Output)]
			public string Salt { get; set; }

			[UsedImplicitly]
			[Length(255)]
			[Direction(ParameterDirection.Output)]
			public string CycleCount { get; set; }
		} // class BrokerLoadDetailsForPasswordUpdate

		private class SpBrokerUpdatePassword : AStoredProc {
			public SpBrokerUpdatePassword(
				int brokerID,
				HashedPassword password,
				AConnection oDB,
				ASafeLog oLog
			) : base(oDB, oLog) {
				BrokerID = brokerID;
				NewPassword = password.Password;
				Salt = password.Salt;
				CycleCount = password.CycleCount;
			} // constructor

			public override bool HasValidParameters() {
				if (BrokerID <= 0)
					return false;

				if (string.IsNullOrWhiteSpace(NewPassword))
					return false;

				if (string.IsNullOrWhiteSpace(Salt))
					return false;

				if (string.IsNullOrWhiteSpace(CycleCount))
					return false;

				return true;
			} // HasValidParameters

			[UsedImplicitly]
			public int BrokerID { get; set; }

			[UsedImplicitly]
			public string NewPassword { get; set; }

			[UsedImplicitly]
			public string Salt { get; set; }

			[UsedImplicitly]
			public string CycleCount { get; set; }
		} // class SpBrokerUpdatePassword
	} // class BrokerUpdatePassword
} // namespace Ezbob.Backend.Strategies.Broker
