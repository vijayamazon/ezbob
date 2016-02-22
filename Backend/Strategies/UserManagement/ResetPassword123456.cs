namespace Ezbob.Backend.Strategies.UserManagement {
	using System.Data;
	using System.Runtime.Serialization;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;
	using MailStrategies;

	[DataContract]
	public enum PasswordResetTarget {
		[EnumMember]
		Customer,

		[EnumMember]
		Broker,
	} // enum PasswordResetTarget

	public class ResetPassword123456 : AStrategy {
		public ResetPassword123456(int nTargetID, PasswordResetTarget nTarget) {
			this.targetType = nTarget;

			this.spLoad = new LoadEmailForPasswordReset(DB, Log) {
				TargetID = nTargetID,
			};
		} // constructor

		public override string Name {
			get { return "ResetPassword123456"; }
		} // Name

		public override void Execute() {
			Log.Debug("Resetting password for user {0}...", this.spLoad.TargetID);

			this.spLoad.ExecuteNonQuery();

			if (string.IsNullOrWhiteSpace(this.spLoad.Email)) {
				Log.Warn("Resetting password for user {0} failed: no email found.", this.spLoad.TargetID);
				return;
			} // if

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			HashedPassword hashed = pu.Generate(this.spLoad.Email, ThePassword);

			var sp = new SavePassword(this.spLoad, DB, Log) {
				Password = hashed.Password,
				Salt = hashed.Salt,
				CycleCount = hashed.CycleCount,
			};

			sp.ExecuteNonQuery();

			Log.Debug("Resetting password for user {0} complete.", this.spLoad.TargetID);

			AStrategy oEmailSender = null;

			switch (this.targetType) {
			case PasswordResetTarget.Customer:
				oEmailSender = new PasswordChanged(this.spLoad.TargetID, ThePassword);
				break;

			case PasswordResetTarget.Broker:
				oEmailSender = new BrokerPasswordChanged(this.spLoad.TargetID, ThePassword);
				break;
			} // switch

			FireToBackground(oEmailSender);
		} // Execute

		private readonly LoadEmailForPasswordReset spLoad;
		private readonly PasswordResetTarget targetType;

		private const string ThePassword = "123456";

		private class LoadEmailForPasswordReset : AStoredProcedure {
			public LoadEmailForPasswordReset(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return TargetID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int TargetID { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			[Length(255)]
			public string Email { get; set; }
		} // class LoadEmailForPasswordReset

		private class SavePassword : AStoredProcedure {
			public SavePassword(LoadEmailForPasswordReset oSpLoad, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				TargetID = oSpLoad.TargetID;
			} // constructor

			public override bool HasValidParameters() {
				return (TargetID > 0) && !string.IsNullOrWhiteSpace(Password);
			} // HasValidParameters

			[UsedImplicitly]
			public int TargetID { get; set; }

			[UsedImplicitly]
			public string Password { get; set; }

			[UsedImplicitly]
			public string Salt { get; set; }

			[UsedImplicitly]
			public string CycleCount { get; set; }
		} // class SavePassword
	} // class ResetPassword123456
} // namespace
