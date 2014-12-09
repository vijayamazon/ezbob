namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using System.Web.Security;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using MailStrategies;

	public class UserLogin : AStrategy {

		public UserLogin(string sEmail, Password oPassword, string sRemoteIp, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oResult = null;

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				OldPassword = oPassword.Primary,
			};

			m_oSpLoad = new UserDataForLogin(DB, Log) {
				Email = sEmail,
			};

			m_oSpResult = new UserLoginCheckResult(DB, Log) {
				Ip = sRemoteIp,
			};
		} // constructor

		public override string Name {
			get { return "User login"; }
		} // Name

		public override void Execute() {
			Log.Debug("User '{0}' tries to log in...", m_oData.Email);

			m_oData.ValidateEmail();
			m_oData.ValidateOldPassword();

			var oUser = m_oSpLoad.FillFirst<UserDataForLogin.Result>();

			Log.Debug("User '{0}': data for check is {1}.", m_oData.Email, oUser);

			if (oUser.Email != m_oData.Email) {
				Log.Debug("User '{0}' not found.", m_oData.Email);
				m_oResult = MembershipCreateStatus.InvalidUserName;
				return;
			} // if

			m_oSpResult.UserID = oUser.UserID;

			bool bContinue = true;

			if (oUser.IsDeleted != 0) {
				Log.Debug("User '{0}' is deleted.", m_oData.Email);
				m_oResult = MembershipCreateStatus.InvalidUserName;
				m_oSpResult.ErrorMessage = "User is deleted.";
				bContinue = false;
			} // if

			if (bContinue && oUser.ForcePassChange.HasValue && oUser.ForcePassChange.Value) {
				Log.Debug("User '{0}' must change password.", m_oData.Email);
				m_oResult = MembershipCreateStatus.UserRejected;
				m_oSpResult.ErrorMessage = "User must change password.";
				bContinue = false;
			} // if

			if (bContinue && oUser.PassSetTime.HasValue && oUser.PassExpPeriod.HasValue && oUser.PassSetTime.Value.Add(TimeSpan.FromSeconds(oUser.PassExpPeriod.Value)) < DateTime.UtcNow) {
				Log.Debug("User '{0}' has expired password.", m_oData.Email);
				m_oResult = MembershipCreateStatus.UserRejected;
				m_oSpResult.ErrorMessage = "Password has expired.";
				bContinue = false;
			} // if

			if (bContinue && oUser.DisableDate.HasValue && oUser.DisableDate.Value < DateTime.UtcNow) {
				Log.Debug("User '{0}' is disabled.", m_oData.Email);
				m_oSpResult.IsDeleted = 2;
				m_oSpResult.ErrorMessage = "User is disabled.";
				bContinue = false;
				m_oResult = MembershipCreateStatus.UserRejected;
			} // if

			if (bContinue && oUser.FailCount > m_oData.Cfg.NumOfInvalidPasswordAttempts) {
				Log.Debug("User '{0}' has failed to log in too many times.", m_oData.Email);
				m_oSpResult.ErrorMessage = "Too many log in attempts.";
				bContinue = false;
				m_oResult = MembershipCreateStatus.UserRejected;
			} // if bContinue

			if (bContinue) {
				string sPasswordStyle = oUser.IsOldPasswordStyle ? "old" : "new";

				if (oUser.IsPasswordValid(m_oData.OldPassword)) {
					Log.Debug(
						"User '{0}': valid password (verified using {1} password style).",
						m_oData.Email,
						sPasswordStyle
					);

					m_oSpResult.EzPassword = oUser.IsOldPasswordStyle ? m_oData.OldPasswordHash : null;
					m_oSpResult.ErrorMessage = null;
					m_oSpResult.Success = true;
					m_oSpResult.LastBadLogin = null;
					m_oSpResult.LoginFailedCount = 0;
					m_oSpResult.IsDeleted = 0;
					m_oResult = MembershipCreateStatus.Success;
				}
				else {
					Log.Debug(
						"User '{0}': invalid password (verified using {1} password style).",
						m_oData.Email,
						sPasswordStyle
					);

					m_oSpResult.LoginFailedCount = oUser.FailCount + 1;
					m_oSpResult.LastBadLogin = DateTime.UtcNow;
					m_oSpResult.ErrorMessage = "Invalid password.";

					m_oResult = m_oSpResult.LoginFailedCount >= m_oData.Cfg.NumOfInvalidPasswordAttempts
						? MembershipCreateStatus.InvalidProviderUserKey
						: MembershipCreateStatus.InvalidPassword;
				} // if
			} // if bContinue

			SessionID = m_oSpResult.ExecuteScalar<int>();

			Log.Debug("User '{0}' is{1} logged in ({2}).", m_oData.Email, m_oResult == MembershipCreateStatus.Success ? "" : " NOT", Result);

			if (m_oSpResult.LoginFailedCount.HasValue && (m_oSpResult.LoginFailedCount >= m_oData.Cfg.NumOfInvalidPasswordAttempts))
				new ThreeInvalidAttempts(m_oSpResult.UserID, DB, Log).Execute();
		} // Execute

		public string Result {
			get { return m_oResult.HasValue ? m_oResult.Value.ToString() : string.Empty; } // get
		} // Result

		public int SessionID { get; private set; } // SessionID

		private MembershipCreateStatus? m_oResult;
		private readonly UserSecurityData m_oData;
		private readonly UserLoginCheckResult m_oSpResult;
		private readonly UserDataForLogin m_oSpLoad;

		private class UserLoginCheckResult : AStoredProcedure {
			public UserLoginCheckResult(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				UserID = 0;
				EzPassword = null;
				IsDeleted = null;
				LastBadLogin = null;
				LoginFailedCount = null;
				Success = false;
				ErrorMessage = null;
				Ip = null;
			} // constructor

			public override bool HasValidParameters() {
				return UserID > 0;
			} // HasValidParameters

			public int UserID { [UsedImplicitly] get; set; }
			public string EzPassword { [UsedImplicitly] get; set; }
			public int? IsDeleted { [UsedImplicitly] get; set; }
			public DateTime? LastBadLogin { [UsedImplicitly] get; set; }
			public int? LoginFailedCount { [UsedImplicitly] get; set; }
			public bool Success { [UsedImplicitly] get; set; }
			public string ErrorMessage { [UsedImplicitly] get; set; }

			public string Ip {
				[UsedImplicitly]
				get { return m_sIp ?? string.Empty; }
				set { m_sIp = value ?? string.Empty; }
			} // Ip

			private string m_sIp;

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class UserLoginCheckResult

	} // class UserLogin
} // namespace EzBob.Backend.Strategies.UserManagement
