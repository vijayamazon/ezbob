namespace EzService {
	using System;
	using System.ServiceModel;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.UserManagement;
	using EZBob.DatabaseLib.Model.Database;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceUserManagement {
		[OperationContract]
		UserLoginActionResult SignupCustomerMultiOrigin(SignupCustomerMultiOriginModel model);

		[OperationContract]
		UserLoginActionResult LoginCustomerMutliOrigin(LoginCustomerMultiOriginModel model);

		[OperationContract]
		ActionMetaData SignupUnderwriterMultiOrigin(string name, DasKennwort password, string role);

		[OperationContract]
		UserLoginActionResult UserLogin(
			CustomerOriginEnum? originID,
			string sEmail,
			DasKennwort sPassword,
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
		StringActionResult CustomerChangePassword(
			string email,
			CustomerOriginEnum origin,
			DasKennwort oldPassword,
			DasKennwort newPassword
		);

		[OperationContract]
		StringActionResult UserUpdateSecurityQuestion(
			string email,
			CustomerOriginEnum origin,
			DasKennwort password,
			int questionID,
			string answer
		);

		[OperationContract]
		StringActionResult UserChangeEmail(int underwriterId, int nUserID, string sNewEmail);

		[OperationContract]
		ActionMetaData MarkSessionEnded(int nSessionID, string sComment, int? userID, int? nCustomerId);

		[OperationContract]
		CustomerDetailsActionResult LoadCustomerByCreatePasswordToken(Guid oToken);

		[OperationContract]
		SetPasswordActionResult SetCustomerPasswordByToken(
			Guid token,
			CustomerOriginEnum origin,
			DasKennwort password,
			DasKennwort passwordAgain,
			bool isBrokerLead,
			string remoteIP
		);

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
		StringListActionResult LoadAllLoginRoles(string login, CustomerOriginEnum? origin, bool ignoreOrigin);

		[OperationContract]
		StringActionResult GetCustomerSecurityQuestion(string email, CustomerOriginEnum origin);

		[OperationContract]
		StringActionResult ValidateSecurityAnswer(string email, CustomerOriginEnum origin, string answer);
	} // interface IEzServiceUserManagement
} // namespace
