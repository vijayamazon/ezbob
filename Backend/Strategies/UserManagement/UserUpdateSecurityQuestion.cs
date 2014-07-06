namespace EzBob.Backend.Strategies.UserManagement {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class UserUpdateSecurityQuestion : AStrategy {
		#region public

		#region constructor

		public UserUpdateSecurityQuestion(
			string sEmail,
			Password oPassword,
			int nQuestionID,
			string sAnswer,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			ErrorMessage = null;

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				OldPassword = oPassword.Primary,
				PasswordQuestion = nQuestionID,
				PasswordAnswer = sAnswer,
			};

			m_oSpLoad = new UserDataForLogin(DB, Log) {
				Email = sEmail,
			};

			m_oSpUpdate = new SpUserUpdateSecurityQuestion(DB, Log) {
				QuestionID = nQuestionID,
				Answer = sAnswer,
			};
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "User update security question"; }
		} // Name

		#endregion property Name

		#region property ErrorMessage

		public string ErrorMessage { get; set; } // ErrorMessage

		#endregion property ErrorMessage

		#region method Execute

		public override void Execute() {
			ErrorMessage = null;

			Log.Debug("User '{0}' tries to change security question...", m_oData.Email);

			m_oData.ValidateEmail();
			m_oData.ValidateOldPassword();

			var oUser = m_oSpLoad.FillFirst<UserDataForLogin.Result>();

			Log.Debug("User '{0}': data for check is {1}.", m_oData.Email, oUser);

			if (oUser.Email != m_oData.Email) {
				Log.Debug("User '{0}' not found.", m_oData.Email);
				ErrorMessage = "User not found.";
				return;
			} // if

			m_oSpUpdate.UserID = oUser.UserID;

			if (!oUser.IsPasswordValid(m_oData.OldPassword)) {
				Log.Debug("User '{0}' has supplied incorrect password.", m_oData.Email);
				ErrorMessage = "Incorrect password.";
				return;
			} // if

			ErrorMessage = m_oSpUpdate.ExecuteScalar<string>();

			Log.Debug("User '{0}' security question has{1} been changed.", m_oData.Email, string.IsNullOrWhiteSpace(ErrorMessage) ? "" : " NOT");
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly UserSecurityData m_oData;
		private readonly UserDataForLogin m_oSpLoad;
		private readonly SpUserUpdateSecurityQuestion m_oSpUpdate;

		#region class SpUserUpdateSecurityQuestion

		private class SpUserUpdateSecurityQuestion : AStoredProc {
			public SpUserUpdateSecurityQuestion(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (UserID > 0) && (QuestionID > 0) && !string.IsNullOrEmpty(Answer);
			} // HasValidParameters

			public int UserID { get; set; }

			public long QuestionID { get; set; }

			public string Answer { get; set; }
		} // class SpUserUpdateSecurityQuestion

		#endregion class SpUserUpdateSecurityQuestion

		#endregion private
	} // class UserUpdateSecurityQuestion
} // namespace EzBob.Backend.Strategies.UserManagement
