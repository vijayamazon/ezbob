namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.UserManagement;

	partial class EzServiceImplementation {
		#region method CustomerSignup

		public UserLoginActionResult CustomerSignup(
			string sEmail,
			string sPassword,
			int nPasswordQuestion,
			string sPasswordAnswer,
			string sRemoteIp
		) {
			UserSignup oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null,
				sEmail, sPassword, nPasswordQuestion, sPasswordAnswer, sRemoteIp
			);

			return new UserLoginActionResult {
				MetaData = oMetaData,
				Status = oInstance.Result,
				SessionID = oInstance.SessionID,
			};
		} // CustomerSignup

		#endregion method CustomerSignup

		#region method UnderwriterSignup

		public ActionMetaData UnderwriterSignup(string name, string password, string roleName) {
			return ExecuteSync<UserSignup>(null, null, name, password, roleName);
		} // UnderwriterSignup

		#endregion method UnderwriterSignup

		#region method UserLogin

		public UserLoginActionResult UserLogin(string sEmail, string sPassword, string sRemoteIp) {
			UserLogin oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, sPassword, sRemoteIp);

			return new UserLoginActionResult {
				MetaData = oMetaData,
				SessionID = oInstance.SessionID,
				Status = oInstance.Result,
			};
		} // UserLogin

		#endregion method UserLogin

		#region method UserResetPassword

		public StringActionResult UserResetPassword(string sEmail) {
			UserResetPassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.Success ? oInstance.Password.Encrypted : string.Empty,
			};
		} // UserResetPassword

		#endregion method UserResetPassword

		#region method UserChangePassword

		public StringActionResult UserChangePassword(string sEmail, string sOldPassword, string sNewPassword, bool bForceChangePassword) {
			UserChangePassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, sOldPassword, sNewPassword, bForceChangePassword);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserChangePassword

		#endregion method UserChangePassword

		#region method CustomerChangePassword

		public StringActionResult CustomerChangePassword(string sEmail, string sOldPassword, string sNewPassword) {
			CustomerChangePassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, sOldPassword, sNewPassword);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // CustomerChangePassword

		#endregion method CustomerChangePassword

		#region method UserUpdateSecurityQuestion

		public StringActionResult UserUpdateSecurityQuestion(string sEmail, string sPassword, int nQuestionID, string sAnswer) {
			UserUpdateSecurityQuestion oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, sPassword, nQuestionID, sAnswer);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserUpdateSecurityQuestion

		#endregion method UserUpdateSecurityQuestion

		#region method UserChangeEmail

		public StringActionResult UserChangeEmail(int nUserID, string sNewEmail) {
			UserChangeEmail oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nUserID, nUserID, nUserID, sNewEmail);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserChangeEmail

		#endregion method UserChangeEmail
	} // class EzServiceImplementation
} // namespace EzService
