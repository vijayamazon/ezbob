namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class UserResetPassword : AStrategy {
		#region public

		#region constructor

		public UserResetPassword(string sEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
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

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "User reset password"; }
		} // Name

		#endregion property Name

		#region property Password

		public SimplePassword Password { get; private set; } // Password

		#endregion property Password

		#region property Success

		public bool Success { get; private set; } // Success

		#endregion property Success

		#region method Execute

		public override void Execute() {
			m_oData.ValidateEmail();
			m_oData.ValidateNewPassword();

			Success = 0 < m_oSp.ExecuteScalar<int>();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpUserResetPassword m_oSp;
		private readonly UserSecurityData m_oData;

		#region class SpUserResetPassword

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

		#endregion class SpUserResetPassword

		#endregion private
	} // class UserResetPassword
} // namespace EzBob.Backend.Strategies.UserManagement
