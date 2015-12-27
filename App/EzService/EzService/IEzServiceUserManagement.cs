namespace EzService {
	using System;
	using System.ServiceModel;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.UserManagement;
	using EZBob.DatabaseLib.Model.Database;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceUserManagement {
		[OperationContract]
		UserLoginActionResult SignupCustomerMutliOrigin(SignupCustomerMultiOriginModel model);

		[OperationContract]
		ActionMetaData UnderwriterSignup(string name, Password password, string role);

		[OperationContract]
		UserLoginActionResult UserLogin(
			CustomerOriginEnum? originID,
			string sEmail,
			string sPassword,
			string sRemoteIp,
			string promotionName,
			DateTime? promotionPageVisitTime
		);

		[OperationContract]
		StringActionResult UserDisable(
			int userID,
			int customerID,
			string email,
			bool unsubscribeFromMailChimp,
			bool changeEmail
		);

		[OperationContract]
		StringActionResult UserResetPassword(string sEmail);

		[OperationContract]
		StringActionResult UserChangePassword(
			string sEmail,
			Password oOldPassword,
			Password oNewPassword,
			bool bForceChangePassword
		);

		[OperationContract]
		StringActionResult CustomerChangePassword(string sEmail, Password oOldPassword, Password oNewPassword);

		[OperationContract]
		StringActionResult UserUpdateSecurityQuestion(string sEmail, Password oPassword, int nQuestionID, string sAnswer);

		[OperationContract]
		StringActionResult UserChangeEmail(int underwriterId, int nUserID, string sNewEmail);

		[OperationContract]
		ActionMetaData MarkSessionEnded(int nSessionID, string sComment, int? nCustomerId);

		[OperationContract]
		CustomerDetailsActionResult LoadCustomerByCreatePasswordToken(Guid oToken);

		[OperationContract]
		IntActionResult SetCustomerPasswordByToken(string sEmail, Password oPassword, Guid oToken, bool bIsBrokerLead);

		[OperationContract]
		ActionMetaData ResetPassword123456(int nUnderwriterID, int nTargetID, PasswordResetTarget nTarget);

		[OperationContract]
		EmailConfirmationTokenActionResult EmailConfirmationGenerate(int nUserID);

		[OperationContract]
		ActionMetaData EmailConfirmationGenerateAndSend(int nUserID, int underwriterId);

		[OperationContract]
		IntActionResult EmailConfirmationCheckOne(Guid oToken);

		[OperationContract]
		ActionMetaData EmailConfirmationConfirmUser(int nUserID, int nUnderwriterID);

		[OperationContract]
		ActionMetaData AddCciHistory(int nCustomerID, int nUnderwriterID, bool bCciMark);

		[OperationContract]
		StringListActionResult LoadAllLoginRoles(string login);
	} // interface IEzServiceUserManagement
} // namespace
