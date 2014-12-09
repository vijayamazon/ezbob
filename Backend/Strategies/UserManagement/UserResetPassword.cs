namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class UserResetPassword : AStrategy {
		public UserResetPassword(string sEmail) {
			Password = new SimplePassword(8, sEmail);

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				NewPassword = Password.RawValue,
			};

			m_oSp = new SpUserResetPassword(DB, Log) {
				Email = sEmail,
				EzPassword = Password.Hash,
			};
		} // constructor

		public override string Name {
			get { return "User reset password"; }
		} // Name

		public SimplePassword Password { get; private set; } // Password

		public bool Success { get; private set; } // Success

		public override void Execute() {
			m_oData.ValidateEmail();
			m_oData.ValidateNewPassword();

			Success = 0 < m_oSp.ExecuteScalar<int>();
		} // Execute

		private readonly SpUserResetPassword m_oSp;
		private readonly UserSecurityData m_oData;

		private class SpUserResetPassword : AStoredProc {
			public SpUserResetPassword(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return
					!string.IsNullOrWhiteSpace(Email) &&
					!string.IsNullOrWhiteSpace(EzPassword);
			} // HasValidParameters

			[UsedImplicitly]
			public string Email { get; set; }

			[UsedImplicitly]
			public string EzPassword { get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now
		} // class SpUserResetPassword

	} // class UserResetPassword
} // namespace Ezbob.Backend.Strategies.UserManagement
