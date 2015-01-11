namespace EzService.EzServiceImplementation {
	using System;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.UserManagement;
	using Ezbob.Backend.Strategies.UserManagement.EmailConfirmation;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {

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

		public ActionMetaData UnderwriterSignup(string name, Password password, string roleName) {
			return ExecuteSync<UserSignup>(null, null, name, password.Primary, roleName);
		} // UnderwriterSignup

		public UserLoginActionResult UserLogin(string sEmail, Password oPassword, string sRemoteIp) {
			UserLogin oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oPassword, sRemoteIp);

			return new UserLoginActionResult {
				MetaData = oMetaData,
				SessionID = oInstance.SessionID,
				Status = oInstance.Result,
				ErrorMessage = oInstance.ErrorMessage,
			};
		} // UserLogin

		public StringActionResult UserResetPassword(string sEmail) {
			UserResetPassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.Success ? oInstance.Password.Encrypted : string.Empty,
			};
		} // UserResetPassword

		public StringActionResult UserChangePassword(string sEmail, Password oOldPassword, Password oNewPassword, bool bForceChangePassword) {
			UserChangePassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oOldPassword, oNewPassword, bForceChangePassword);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserChangePassword

		public StringActionResult CustomerChangePassword(string sEmail, Password oOldPassword, Password oNewPassword) {
			CustomerChangePassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oOldPassword, oNewPassword);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // CustomerChangePassword

		public StringActionResult UserUpdateSecurityQuestion(string sEmail, Password oPassword, int nQuestionID, string sAnswer) {
			UserUpdateSecurityQuestion oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oPassword, nQuestionID, sAnswer);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserUpdateSecurityQuestion

		public StringActionResult UserChangeEmail(int underwriterId, int nUserID, string sNewEmail) {
			UserChangeEmail oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nUserID, underwriterId, nUserID, sNewEmail);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserChangeEmail

		public ActionMetaData MarkSessionEnded(int nSessionID, string sComment, int? nCustomerId)
		{
			return Execute<MarkSessionEnded>(nCustomerId, null, nSessionID, sComment);
		} // MarkSessionEnded

		public CustomerDetailsActionResult LoadCustomerByCreatePasswordToken(Guid oToken) {
			LoadCustomerByCreatePasswordToken oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, oToken);

			return new CustomerDetailsActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		} // LoadCustomerByCreatePasswordToken

		public IntActionResult SetCustomerPasswordByToken(string sEmail, Password oPassword, Guid oToken, bool bIsBrokerLead) {
			SetCustomerPasswordByToken oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, oPassword, oToken, bIsBrokerLead);

			return new IntActionResult {
				MetaData = oMetaData,
				Value = oInstance.CustomerID,
			};
		} // SetCustomerPasswordByToken

		public ActionMetaData ResetPassword123456(int nUnderwriterID, int nTargetID, PasswordResetTarget nTarget) {
			return Execute<ResetPassword123456>(null, nUnderwriterID, nTargetID, nTarget);
		} // ResetPassword123456

		public EmailConfirmationTokenActionResult EmailConfirmationGenerate(int nUserID) {
			EmailConfirmationGenerate oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nUserID, null, nUserID);

			return new EmailConfirmationTokenActionResult {
				MetaData = oMetaData,
				Token = oInstance.Token,
				Address = oInstance.Address,
			};
		} // EmailConfirmationGenerate

		public ActionMetaData EmailConfirmationGenerateAndSend(int nUserID, int underwriterId) {
			EmailConfirmationGenerate oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nUserID, underwriterId, nUserID);

			if (string.IsNullOrWhiteSpace(oInstance.Address))
				return oMetaData;

			return Execute<SendEmailVerification>(nUserID, null, nUserID, oInstance.Address);
		} // EmailConfirmationGenerateAndSend

		public IntActionResult EmailConfirmationCheckOne(Guid oToken) {
			EmailConfirmationCheckOne oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, oToken);

			return new IntActionResult {
				MetaData = oMetaData,
				Value = (int)oInstance.Response,
			};
		} // EmailConfirmationCheckOne

		public ActionMetaData EmailConfirmationConfirmUser(int nUserID, int nUnderwriterID) {
			return Execute<EmailConfirmationConfirmUser>(nUserID, nUnderwriterID, nUserID);
		} // EmailConfirmationConfirmUser

		public ActionMetaData AddCciHistory(int nCustomerID, int nUnderwriterID, bool bCciMark) {
			return Execute<AddCciHistory>(nCustomerID, nUnderwriterID, nCustomerID, nUnderwriterID, bCciMark);
		} // AddCciHistory

	} // class EzServiceImplementation
} // namespace EzService
