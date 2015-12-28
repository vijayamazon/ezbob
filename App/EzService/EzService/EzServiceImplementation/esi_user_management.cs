namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.UserManagement;
	using Ezbob.Backend.Strategies.UserManagement.EmailConfirmation;
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;

	partial class EzServiceImplementation {
		public UserLoginActionResult SignupCustomerMutliOrigin(SignupCustomerMultiOriginModel model) {
			SignupCustomerMutliOrigin instance;

			ActionMetaData amd = ExecuteSync(out instance, null, null, model);

			return new UserLoginActionResult {
				MetaData = amd,
				Status = instance.Status.ToString(),
				SessionID = instance.SessionID,
				ErrorMessage = instance.ErrorMsg,
				RefNumber = instance.RefNumber,
			};
		} // SignupCustomerMutliOrigin

		public UserLoginActionResult LoginCustomerMutliOrigin(LoginCustomerMultiOriginModel model) {
			LoginCustomerMutliOrigin instance;

			ActionMetaData amd = ExecuteSync(out instance, null, null, model);

			return new UserLoginActionResult {
				MetaData = amd,
				Status = instance.Status.ToString(),
				SessionID = instance.SessionID,
				ErrorMessage = instance.ErrorMsg,
				RefNumber = instance.RefNumber,
			};
		} // LoginCustomerMutliOrigin

		public ActionMetaData UnderwriterSignup(string name, Password password, string roleName) {
			return ExecuteSync<UserSignup>(null, null, name, password.Primary, roleName);
		} // UnderwriterSignup

		public UserLoginActionResult UserLogin(
			CustomerOriginEnum? originID,
			string sEmail,
			DasKennwort sPassword,
			string sRemoteIp,
			string promotionName,
			DateTime? promotionPageVisitTime
		) {
			UserLogin oInstance;

			ActionMetaData oMetaData = ExecuteSync(
				out oInstance,
				null,
				null,
				originID,
				sEmail,
				sPassword,
				sRemoteIp,
				promotionName,
				promotionPageVisitTime
			);

			return new UserLoginActionResult {
				MetaData = oMetaData,
				SessionID = oInstance.SessionID,
				Status = oInstance.Result,
				ErrorMessage = oInstance.ErrorMessage,
			};
		} // UserLogin

        public StringActionResult UserDisable(
            int userID,
            int customerID,
            string email,
            bool unsubscribeFromMailChimp,
            bool changeEmail
        ) {
            UserDisable udInstance;

            ActionMetaData udMetaData = ExecuteSync(out udInstance, customerID, userID, customerID, email, unsubscribeFromMailChimp);

            if (changeEmail) {
                UserChangeEmail uceInstance;

                ActionMetaData oMetaData = ExecuteSync(out uceInstance, customerID, userID, customerID, string.Format("{0}frozen", email));

                return new StringActionResult {
                    MetaData = oMetaData,
                    Value = uceInstance.ErrorMessage,
                };
            }

            return new StringActionResult {
                MetaData = udMetaData
            };
        }

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

		public ActionMetaData MarkSessionEnded(int nSessionID, string sComment, int? nCustomerId) {
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
			};
		} // EmailConfirmationGenerate

		public ActionMetaData EmailConfirmationGenerateAndSend(int nUserID, int underwriterId) {
			EmailConfirmationGenerate oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nUserID, underwriterId, nUserID);
			if (oMetaData.Status == ActionStatus.Done || oMetaData.Status == ActionStatus.Finished) {
				return Execute<SendEmailVerification>(nUserID, null, nUserID, oInstance.Token.ToString());
			}
			return oMetaData;
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

		public StringListActionResult LoadAllLoginRoles(string login) {
			LoadAllLoginRoles instance;

			ActionMetaData amd = ExecuteSync(out instance, null, null, login);

			return new StringListActionResult {
				MetaData = amd,
				Records = new List<string>(instance.Roles),
			};
		} // LoadAllLoginRoles
	} // class EzServiceImplementation
} // namespace EzService
