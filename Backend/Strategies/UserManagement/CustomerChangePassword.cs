namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class CustomerChangePassword : AStrategy {
		public CustomerChangePassword(
			string email,
			CustomerOriginEnum? origin,
			DasKennwort oldPassword,
			DasKennwort newPassword
		) {
			ErrorMessage = null;

			this.securityData = new UserSecurityData(this) {
				Email = email,
				OldPassword = oldPassword.Decrypt(),
				NewPassword = newPassword.Decrypt(),
			};

			this.spLoad = new UserDataForLogin(DB, Log) {
				Email = email,
				OriginID = origin == null ? (int?)null : (int)origin.Value
			};

			this.spResult = new SpUserChangePassword(DB, Log);
		} // constructor

		public override string Name {
			get { return "Customer change password"; }
		} // Name

		public override void Execute() {
			ErrorMessage = null;

			string userDisplay = string.Format("{0} with origin {1}", this.securityData.Email, this.spLoad.OriginID);

			this.securityData.ValidateEmail();
			this.securityData.ValidateOldPassword();
			this.securityData.ValidateNewPassword();

			var oUser = this.spLoad.FillFirst<UserDataForLogin.Result>();

			Log.Debug("User '{0}': data for check is {1}.", userDisplay, oUser);

			if (oUser.Email != this.securityData.Email) {
				Log.Debug("User '{0}' not found.", this.securityData.Email);
				ErrorMessage = "Invalid user name.";
				return;
			} // if

			if (this.securityData.OldPassword == this.securityData.NewPassword) {
				Log.Debug("User '{0}': current and new passwords are the same.", userDisplay);
				ErrorMessage = "Current and new passwords are the same.";
				return;
			} // if

			if (oUser.DisablePassChange.HasValue && oUser.DisablePassChange.Value) {
				Log.Debug("User '{0}': changing password is disabled.", userDisplay);
				ErrorMessage = "Changing password is disabled.";
				return;
			} // if

			if (!oUser.IsPasswordValid(this.securityData.OldPassword)) {
				Log.Debug("User '{0}': current password does not match.", userDisplay);
				ErrorMessage = "Current password does not match.";
				return;
			} // if

			UserID = oUser.UserID;

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			var hashed = pu.Generate(oUser.Email, this.securityData.NewPassword);

			this.spResult.UserID = oUser.UserID;
			this.spResult.EzPassword = hashed.Password;
			this.spResult.Salt = hashed.Salt;
			this.spResult.CycleCount = hashed.CycleCount;

			ErrorMessage = this.spResult.ExecuteScalar<string>();

			Log.Debug(
				"User '{0}' password has{1} been changed. {2}",
				userDisplay,
				string.IsNullOrWhiteSpace(ErrorMessage) ? "" : " NOT",
				ErrorMessage
			);

			if (string.IsNullOrWhiteSpace(ErrorMessage))
				FireToBackground(new PasswordChanged(UserID, RawPassword));
		} // Execute

		public string ErrorMessage { get; private set; } // ErrorMessage

		protected int UserID { get; private set; } // UserID

		protected string RawPassword {
			get { return this.securityData.NewPassword; }
		} // Password

		private readonly UserSecurityData securityData;
		private readonly UserDataForLogin spLoad;
		private readonly SpUserChangePassword spResult;

		private class SpUserChangePassword : AStoredProc {
			public SpUserChangePassword(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return
					(UserID > 0) &&
					!string.IsNullOrWhiteSpace(EzPassword) &&
					!string.IsNullOrWhiteSpace(Salt) &&
					!string.IsNullOrWhiteSpace(CycleCount);
			} // HasValidParameters

			public int UserID { [UsedImplicitly] get; set; }

			public string EzPassword { [UsedImplicitly] get; set; }
			public string Salt { [UsedImplicitly] get; set; }
			public string CycleCount { [UsedImplicitly] get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class SpUserChangePassword
	} // class CustomerChangePassword
} // namespace Ezbob.Backend.Strategies.UserManagement
