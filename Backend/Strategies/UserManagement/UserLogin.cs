namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Web.Security;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class UserLogin : ASignupLoginBaseStrategy {
		public UserLogin(
			CustomerOriginEnum? originID,
			string sEmail,
			DasKennwort sPassword,
			string sRemoteIp,
			string promotionName,
			DateTime? promotionPageVisitTime
		) {
			m_oResult = null;

			sEmail = NormalizeUserName(sEmail);

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				OldPassword = sPassword.Decrypt(),
			};

			this.originName = originID.HasValue ? originID.Value.ToString() : "-- null --";

			m_oSpLoad = new UserDataForLogin(DB, Log) {
				Email = sEmail,
				OriginID = originID.HasValue ? (int)originID.Value : (int?)null,
			};

			m_oSpResult = new UserLoginCheckResult(DB, Log) {
				Ip = sRemoteIp,
				LotteryCode = promotionName,
				PageVisitTime = promotionPageVisitTime,
			};
		} // constructor

		public override string Name {
			get { return "User login"; }
		} // Name

		public override void Execute() {
			Log.Debug("User '{0}' with origin '{1}' tries to log in...", m_oData.Email, this.originName);

			m_oData.ValidateEmail();
			m_oData.ValidateOldPassword();

			var oUser = m_oSpLoad.FillFirst<UserDataForLogin.Result>();

			Log.Debug("User '{0}' with origin '{1}': data for check is {2}.", m_oData.Email, this.originName, oUser);

			if (!oUser.MatchesEnteredEmail(m_oData.Email)) {
				Log.Debug("User '{0}' with origin {1} not found.", m_oData.Email, this.originName);
				m_oResult = MembershipCreateStatus.InvalidUserName;
				return;
			} // if

			m_oSpResult.UserID = oUser.UserID;

			bool bContinue = true;

			if (oUser.IsDeleted != 0) {
				Log.Debug("User '{0}' with origin '{1}' is deleted.", m_oData.Email, this.originName);
				m_oResult = MembershipCreateStatus.InvalidUserName;
				m_oSpResult.ErrorMessage = "User is deleted.";
				bContinue = false;
			} // if

			if (bContinue && oUser.ForcePassChange.HasValue && oUser.ForcePassChange.Value) {
				Log.Debug("User '{0}' with origin '{1}' must change password.", m_oData.Email, this.originName);
				m_oResult = MembershipCreateStatus.UserRejected;
				m_oSpResult.ErrorMessage = "User must change password.";
				bContinue = false;
			} // if

			if (bContinue && oUser.PassSetTime.HasValue && oUser.PassExpPeriod.HasValue && oUser.PassSetTime.Value.Add(TimeSpan.FromSeconds(oUser.PassExpPeriod.Value)) < DateTime.UtcNow) {
				Log.Debug("User '{0}' with origin '{1}' has expired password.", m_oData.Email, this.originName);
				m_oResult = MembershipCreateStatus.UserRejected;
				m_oSpResult.ErrorMessage = "Password has expired.";
				bContinue = false;
			} // if

			if (bContinue && oUser.DisableDate.HasValue && oUser.DisableDate.Value < DateTime.UtcNow) {
				Log.Debug("User '{0}' with origin '{1}' is disabled.", m_oData.Email, this.originName);
				m_oSpResult.IsDeleted = 2;
				m_oSpResult.ErrorMessage = "User is disabled.";
				bContinue = false;
				m_oResult = MembershipCreateStatus.UserRejected;
			} // if

			if (bContinue && oUser.FailCount > m_oData.Cfg.NumOfInvalidPasswordAttempts) {
				Log.Debug(
					"User '{0}' with origin '{1}' has failed to log in too many times.",
					m_oData.Email,
					this.originName
				);
				m_oSpResult.ErrorMessage = "Too many log in attempts.";
				bContinue = false;
				m_oResult = MembershipCreateStatus.UserRejected;
			} // if bContinue

			if (bContinue) {
				string sPasswordStyle = oUser.IsOldPasswordStyle ? "old" : "new";

				if (oUser.IsPasswordValid(m_oData.OldPassword)) {
					Log.Debug(
						"User '{0}' with origin '{1}': valid password (verified using {2} password style).",
						m_oData.Email,
						this.originName,
						sPasswordStyle
					);

					if (oUser.RenewStoredPassword == null) {
						m_oSpResult.EzPassword = null;
						m_oSpResult.Salt = null;
						m_oSpResult.CycleCount = null;
					} else {
						m_oSpResult.EzPassword = oUser.RenewStoredPassword.Password;
						m_oSpResult.Salt = oUser.RenewStoredPassword.Salt;
						m_oSpResult.CycleCount = oUser.RenewStoredPassword.CycleCount;
					} // if

					m_oSpResult.ErrorMessage = null;
					m_oSpResult.Success = true;
					m_oSpResult.LastBadLogin = null;
					m_oSpResult.LoginFailedCount = 0;
					m_oSpResult.IsDeleted = 0;
					m_oResult = MembershipCreateStatus.Success;
				} else {
					Log.Debug(
						"User '{0}' with origin '{1}': invalid password (verified using {2} password style).",
						m_oData.Email,
						this.originName,
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

			Log.Debug(
				"User '{0}' with origin '{1}' is{2} logged in ({3}).",
				m_oData.Email,
				this.originName,
				m_oResult == MembershipCreateStatus.Success ? "" : " NOT",
				Result
			);

			bool tooManyAttempts =
				m_oSpResult.LoginFailedCount.HasValue &&
				(m_oSpResult.LoginFailedCount >= m_oData.Cfg.NumOfInvalidPasswordAttempts);

			if (tooManyAttempts) {
				try {
					new ThreeInvalidAttempts(m_oSpResult.UserID).Execute();
				} catch (Exception e) {
					Log.Alert(e, "Failed to fully process 'Three invalid attempts' password generation and  email.");
				} // try
			} // if
		} // Execute

		public string Result {
			get { return m_oResult.HasValue ? m_oResult.Value.ToString() : string.Empty; } // get
		} // Result

		public string ErrorMessage { get { return m_oSpResult.ErrorMessage; } }

		public int SessionID { get; private set; } // SessionID

		public MembershipCreateStatus Status {
			get { return this.m_oResult ?? MembershipCreateStatus.ProviderError; }
		} // Status

		private MembershipCreateStatus? m_oResult;
		private readonly UserSecurityData m_oData;
		private readonly UserLoginCheckResult m_oSpResult;
		private readonly UserDataForLogin m_oSpLoad;
		private readonly string originName;

		private class UserLoginCheckResult : AStoredProcedure {
			public UserLoginCheckResult(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			} // constructor

			public override bool HasValidParameters() {
				return UserID > 0;
			} // HasValidParameters

			public int UserID { [UsedImplicitly] get; set; }
			public string EzPassword { [UsedImplicitly] get; set; }
			public string Salt { [UsedImplicitly] get; set; }
			public string CycleCount { [UsedImplicitly] get; set; }
			public int? IsDeleted { [UsedImplicitly] get; set; }
			public DateTime? LastBadLogin { [UsedImplicitly] get; set; }
			public int? LoginFailedCount { [UsedImplicitly] get; set; }
			public bool Success { [UsedImplicitly] get; set; }
			public string ErrorMessage { [UsedImplicitly] get; set; }
			public string LotteryCode { [UsedImplicitly] get; set; }
			public DateTime? PageVisitTime { [UsedImplicitly] get; set; }

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
} // namespace Ezbob.Backend.Strategies.UserManagement
