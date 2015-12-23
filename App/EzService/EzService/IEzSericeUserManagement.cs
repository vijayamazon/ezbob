﻿namespace EzService {
	using System;
	using System.ServiceModel;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.UserManagement;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzSericeUserManagement {
		[OperationContract]
		UserLoginActionResult SignupCustomerMutliOrigin(SignupMultiOriginModel model);

		[OperationContract]
		UserLoginActionResult CustomerSignup(
			string sEmail,
			Password oPassword,
			int nPasswordQuestion,
			string sPasswordAnswer,
			string sRemoteIp
		);

		[OperationContract]
		ActionMetaData UnderwriterSignup(string name, Password password, string role);

		[OperationContract]
		UserLoginActionResult UserLogin(
			string sEmail,
			Password sPassword,
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
	} // interface IEzSericeUserManagement
} // namespace
