namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.UserManagement;
	using Ezbob.Backend.Strategies.UserManagement.EmailConfirmation;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using EzService.ActionResults;
	using EZBob.DatabaseLib.Model.Database;

	partial class EzServiceImplementation {
		public UserLoginActionResult SignupCustomerMultiOrigin(SignupCustomerMultiOriginModel model) {
			SignupCustomerMultiOrigin instance;

			ActionMetaData amd = ExecuteSync(out instance, null, null, model);

			return new UserLoginActionResult {
				MetaData = amd,
				Status = instance.Status.ToString(),
				SessionID = instance.SessionID,
				ErrorMessage = instance.ErrorMsg,
				RefNumber = instance.RefNumber,
			};
		} // SignupCustomerMultiOrigin

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

		public ActionMetaData SignupUnderwriterMultiOrigin(string name, DasKennwort password, string roleName) {
			return ExecuteSync<SignupUnderwriterMultiOrigin>(null, null, name, password, roleName);
		} // SignupUnderwriterMultiOrigin

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

			ActionMetaData udMetaData = ExecuteSync(
				out udInstance,
				customerID,
				userID,
				customerID,
				email,
				unsubscribeFromMailChimp
			);

			if (changeEmail) {
				UserChangeEmail uceInstance;

				ActionMetaData oMetaData = ExecuteSync(
					out uceInstance,
					customerID,
					userID,
					userID,
					customerID,
					string.Format("{0}frozen", email)
				);

				return new StringActionResult {
					MetaData = oMetaData,
					Value = uceInstance.ErrorMessage,
				};
			}

			return new StringActionResult {
				MetaData = udMetaData
			};
		}

		public StringActionResult CustomerChangePassword(
			string email,
			CustomerOriginEnum origin,
			DasKennwort oldPassword,
			DasKennwort newPassword
		) {
			CustomerChangePassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(
				out oInstance,
				null,
				null,
				email,
				origin,
				oldPassword,
				newPassword
			);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // CustomerChangePassword

		public StringActionResult UserUpdateSecurityQuestion(
			string email,
			CustomerOriginEnum origin,
			DasKennwort password,
			int questionID,
			string answer
		) {
			UserUpdateSecurityQuestion oInstance;

			ActionMetaData oMetaData = ExecuteSync(
				out oInstance,
				null,
				null,
				email,
				origin,
				password,
				questionID,
				answer
			);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserUpdateSecurityQuestion

		public StringActionResult UserChangeEmail(int underwriterId, int nUserID, string sNewEmail) {
			UserChangeEmail oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nUserID, underwriterId, underwriterId, nUserID, sNewEmail);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.ErrorMessage,
			};
		} // UserChangeEmail

		public ActionMetaData MarkSessionEnded(int nSessionID, string sComment, int? userID, int? nCustomerId) {
			return Execute<MarkSessionEnded>(nCustomerId, userID, nSessionID, sComment);
		} // MarkSessionEnded

		public CustomerDetailsActionResult LoadCustomerByCreatePasswordToken(Guid oToken) {
			LoadCustomerByCreatePasswordToken oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, oToken);

			return new CustomerDetailsActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		} // LoadCustomerByCreatePasswordToken

		public SetPasswordActionResult SetCustomerPasswordByToken(
			Guid token,
			CustomerOriginEnum origin,
			DasKennwort password,
			DasKennwort passwordAgain,
			bool isBrokerLead,
			string remoteIP
		) {
			SetCustomerPasswordByToken instance;

			ActionMetaData oMetaData = ExecuteSync(
				out instance,
				null,
				null,
				token,
				origin,
				password,
				passwordAgain,
				isBrokerLead,
				remoteIP
			);

			return new SetPasswordActionResult {
				MetaData = oMetaData,
				ErrorMsg = instance.ErrorMsg,
				UserID = instance.UserID,
				IsBroker = instance.IsBroker,
				Email = instance.Email,
				SessionID = instance.SessionID,
				IsDisabled = instance.IsDisabled,
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

		public StringListActionResult LoadAllLoginRoles(string login, CustomerOriginEnum? origin, bool ignoreOrigin) {
			LoadAllLoginRoles instance;

			ActionMetaData amd = ExecuteSync(out instance, null, null, login, origin, ignoreOrigin);

			return new StringListActionResult {
				MetaData = amd,
				Records = new List<string>(instance.Roles),
			};
		} // LoadAllLoginRoles

		public StringActionResult GetCustomerSecurityQuestion(string email, CustomerOriginEnum origin) {
			GetCustomerSecurityQuestion instance;

			ActionMetaData amd = ExecuteSync(out instance, null, null, email, origin);

			return new StringActionResult {
				MetaData = amd,
				Value = instance.SecurityQuestion,
			};
		} // GetCustomerSecurityQuestion

		public StringActionResult ValidateSecurityAnswer(string email, CustomerOriginEnum origin, string answer) {
			ValidateSecurityAnswer instance;

			ActionMetaData amd = ExecuteSync(out instance, null, null, email, origin, answer);

			return new StringActionResult {
				MetaData = amd,
				Value = instance.ErrorMsg,
			};
		} // ValidateSecurityAnswer
	} // class EzServiceImplementation
} // namespace EzService
