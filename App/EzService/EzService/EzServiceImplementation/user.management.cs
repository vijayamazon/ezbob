namespace EzService.EzServiceImplementation {
	using System;
	using EzBob.Backend.Strategies.MailStrategies;
	using EzBob.Backend.Strategies.UserManagement;
	using EzBob.Backend.Strategies.UserManagement.EmailConfirmation;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		#region method CustomerSignup

		public UserLoginActionResult CustomerSignup(
			string sEmail,
			Password oPassword,
			int nPasswordQuestion,
			string sPasswordAnswer,
			string sRemoteIp
		) {
			UserSignup oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null,
				sEmail, oPassword, nPasswordQuestion, sPasswordAnswer, sRemoteIp
			);

			return new UserLoginActionResult {
				MetaData = oMetaData,
				Status = oInstance.Result,
				SessionID = oInstance.SessionID,
			};
		} // CustomerSignup

		#endregion method CustomerSignup

		#region method UnderwriterSignup

		public ActionMetaData UnderwriterSignup(string name, Password password, string roleName) {
			return ExecuteSync<UserSignup>(null, null, name, password, roleName);
		} // UnderwriterSignup

		#endregion method UnderwriterSignup

		#region method UserLogin

		public UserLoginActionResult UserLogin(string sEmail, Password oPassword, string sRemoteIp) {
			UserLogin oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oPassword, sRemoteIp);

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

		public StringActionResult UserChangePassword(string sEmail, Password oOldPassword, Password oNewPassword, bool bForceChangePassword) {
			UserChangePassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oOldPassword, oNewPassword, bForceChangePassword);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserChangePassword

		#endregion method UserChangePassword

		#region method CustomerChangePassword

		public StringActionResult CustomerChangePassword(string sEmail, Password oOldPassword, Password oNewPassword) {
			CustomerChangePassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oOldPassword, oNewPassword);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // CustomerChangePassword

		#endregion method CustomerChangePassword

		#region method UserUpdateSecurityQuestion

		public StringActionResult UserUpdateSecurityQuestion(string sEmail, Password oPassword, int nQuestionID, string sAnswer) {
			UserUpdateSecurityQuestion oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oPassword, nQuestionID, sAnswer);

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

		#region method MarkSessionEnded

		public ActionMetaData MarkSessionEnded(int nSessionID) {
			return Execute<MarkSessionEnded>(null, null, nSessionID);
		} // MarkSessionEnded

		#endregion method MarkSessionEnded

		#region method LoadCustomerByCreatePasswordToken

		public CustomerDetailsActionResult LoadCustomerByCreatePasswordToken(Guid oToken) {
			LoadCustomerByCreatePasswordToken oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, oToken);

			return new CustomerDetailsActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		} // LoadCustomerByCreatePasswordToken

		#endregion method LoadCustomerByCreatePasswordToken

		#region method SetCustomerPasswordByToken

		public IntActionResult SetCustomerPasswordByToken(string sEmail, Password oPassword, Guid oToken, bool bIsBrokerLead) {
			SetCustomerPasswordByToken oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oPassword, oToken, bIsBrokerLead);

			return new IntActionResult {
				MetaData = oMetaData,
				Value = oInstance.CustomerID,
			};
		} // SetCustomerPasswordByToken

		#endregion method SetCustomerPasswordByToken

		#region method ResetPassword123456

		public ActionMetaData ResetPassword123456(int nUnderwriterID, int nTargetID, PasswordResetTarget nTarget) {
			return Execute<ResetPassword123456>(null, nUnderwriterID, nTargetID, nTarget);
		} // ResetPassword123456

		#endregion method ResetPassword123456

		#region method EmailConfirmationGenerate

		public EmailConfirmationTokenActionResult EmailConfirmationGenerate(int nUserID) {
			EmailConfirmationGenerate oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nUserID, null, nUserID);

			return new EmailConfirmationTokenActionResult {
				MetaData = oMetaData,
				Token = oInstance.Token,
				Address = oInstance.Address,
			};
		} // EmailConfirmationGenerate

		#endregion method EmailConfirmationGenerate

		#region method EmailConfirmationGenerateAndSend

		public ActionMetaData EmailConfirmationGenerateAndSend(int nUserID) {
			EmailConfirmationGenerate oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nUserID, null, nUserID);

			if (string.IsNullOrWhiteSpace(oInstance.Address))
				return oMetaData;

			return Execute<SendEmailVerification>(nUserID, null, nUserID, oInstance.Address);
		} // EmailConfirmationGenerateAndSend

		#endregion method EmailConfirmationGenerateAndSend

		#region method EmailConfirmationCheckOne

		public IntActionResult EmailConfirmationCheckOne(Guid oToken) {
			EmailConfirmationCheckOne oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, oToken);

			return new IntActionResult {
				MetaData = oMetaData,
				Value = (int)oInstance.Response,
			};
		} // EmailConfirmationCheckOne

		#endregion method EmailConfirmationCheckOne

		#region method EmailConfirmationConfirmUser

		public ActionMetaData EmailConfirmationConfirmUser(int nUserID, int nUnderwriterID) {
			return Execute<EmailConfirmationConfirmUser>(nUserID, nUnderwriterID, nUserID);
		} // EmailConfirmationConfirmUser

		#endregion method EmailConfirmationConfirmUser
	} // class EzServiceImplementation
} // namespace EzService
