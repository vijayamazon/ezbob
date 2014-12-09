﻿namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class UserChangePassword : AStrategy {

		public UserChangePassword(string sEmail, Password oOldPassword, Password oNewPassword, bool bForceChangePassword, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_bForceChangePassword = bForceChangePassword;
			ErrorMessage = null;

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				OldPassword = oOldPassword.Primary,
				NewPassword = oNewPassword.Primary,
			};

			m_oSpLoad = new UserDataForLogin(DB, Log) {
				Email = sEmail,
			};

			m_oSpResult = new SpUserChangePassword(DB, Log);
		} // constructor

		public override string Name {
			get { return "User change password"; }
		} // Name

		public override void Execute() {
			ErrorMessage = null;

			m_oData.ValidateEmail();
			m_oData.ValidateOldPassword();
			m_oData.ValidateNewPassword();

			var oUser = m_oSpLoad.FillFirst<UserDataForLogin.Result>();

			Log.Debug("User '{0}': data for check is {1}.", m_oData.Email, oUser);

			if (oUser.Email != m_oData.Email) {
				Log.Debug("User '{0}' not found.", m_oData.Email);
				ErrorMessage = "Invalid user name.";
				return;
			} // if

			if (m_oData.OldPassword == m_oData.NewPassword) {
				Log.Debug("User '{0}': current and new passwords are the same.", m_oData.Email);
				ErrorMessage = "Current and new passwords are the same.";
				return;
			} // if

			if (!m_bForceChangePassword) {
				if (oUser.DisablePassChange.HasValue && oUser.DisablePassChange.Value) {
					Log.Debug("User '{0}': changing password is disabled.", m_oData.Email);
					ErrorMessage = "Changing password is disabled.";
					return;
				} // if

				if (!oUser.IsPasswordValid(m_oData.OldPassword)) {
					Log.Debug("User '{0}': current password does not match.", m_oData.Email);
					ErrorMessage = "Current password does not match.";
					return;
				} // if
			} // if

			UserID = oUser.UserID;

			m_oSpResult.UserID = oUser.UserID;
			m_oSpResult.EzPassword = m_oData.NewPasswordHash;

			ErrorMessage = m_oSpResult.ExecuteScalar<string>();

			Log.Debug(
				"User '{0}' password has{1} been changed. {2}",
				m_oData.Email,
				string.IsNullOrWhiteSpace(ErrorMessage) ? "" : " NOT",
				ErrorMessage
			);
		} // Execute

		public string ErrorMessage { get; private set; } // ErrorMessage

		protected int UserID { get; private set; } // UserID

		protected string Password {
			get { return m_oData.NewPassword; }
		} // Password

		private readonly UserSecurityData m_oData;
		private readonly bool m_bForceChangePassword;
		private readonly UserDataForLogin m_oSpLoad;
		private readonly SpUserChangePassword m_oSpResult;

		private class SpUserChangePassword : AStoredProc {
			public SpUserChangePassword(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (UserID > 0) && !string.IsNullOrWhiteSpace(EzPassword);
			} // HasValidParameters

			public int UserID { [UsedImplicitly] get; set; }

			public string EzPassword { [UsedImplicitly] get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class SpUserChangePassword

	} // class UserChangePassword
} // namespace EzBob.Backend.Strategies.UserManagement
