namespace EzService {
	using System;
	using System.ServiceModel;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using EzService.ActionResults;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceBroker {
		[OperationContract]
		ActionMetaData BrokerAcceptTerms(int nTermsID, string sContactEmail);

		[OperationContract]
		ActionMetaData BrokerAddCustomerLead(
			string sLeadFirstName,
			string sLeadLastName,
			string sLeadEmail,
			string sLeadAddMode,
			string sContactEmail
		);

		[OperationContract]
		ActionMetaData BrokerAddBank(BrokerAddBankModel model);

		[OperationContract]
		ActionMetaData BrokerApproveAndResetCustomerPassword(
			int nUnderwriterID,
			int nCustomerID,
			decimal nLoanAmount,
			int nValidHours,
			bool isFirst
		);

		[OperationContract]
		ActionMetaData BrokerAttachCustomer(int nCustomerID, int? nBrokerID, int nUnderwriterID);

		[OperationContract]
		StringActionResult BrokerBackFromCustomerWizard(int nLeadID);

		[OperationContract]
		ActionMetaData BrokerCheckCustomerRelevance(
			int nCustomerID,
			string sCustomerEmail,
			bool isAlibaba,
			string sSourceRef,
			string sConfirmationToken
		);

		[OperationContract]
		ActionMetaData BrokerCustomerWizardComplete(int nCustomerID);

		[OperationContract]
		ActionMetaData BrokerDeleteCustomerFiles(string sCustomerRefNum, string sContactEmail, int[] aryFileIDs);

		[OperationContract]
		BrokerCustomerFileContentsActionResult BrokerDownloadCustomerFile(
			string sCustomerRefNum,
			string sContactEmail,
			int nFileID
		);

		[OperationContract]
		ActionMetaData BrokerForceResetCustomerPassword(int nUserID, int nCustomerID);

		[OperationContract]
		BrokerInstantOfferResponseActionResult BrokerInstantOffer(BrokerInstantOfferRequest request);

		[OperationContract]
		ActionMetaData BrokerLeadAcquireCustomer(
			int nCustomerID,
			int nLeadID,
			string sFirstName,
			bool bBrokerFillsForCustomer,
			string sConfirmationToken
		);

		[OperationContract]
		BrokerLeadDetailsActionResult BrokerLeadCanFillWizard(int nLeadID, string sLeadEmail, string sContactEmail);

		[OperationContract]
		BrokerLeadDetailsActionResult BrokerLeadCheckToken(string sToken);

		[OperationContract]
		ActionMetaData BrokerLeadSendInvitation(int nLeadID, string sBrokerContactEmail);

		[OperationContract]
		BrokerCustomerDetailsActionResult BrokerLoadCustomerDetails(string sCustomerRefNum, string sContactEmail);

		[OperationContract]
		BrokerLeadDetailsDataActionResult BrokerLoadLeadDetails(int leadID, string sContactEmail);

		[OperationContract]
		BrokerCustomerFilesActionResult BrokerLoadCustomerFiles(string sCustomerRefNum, string sContactEmail);

		[OperationContract]
		BrokerCustomersActionResult BrokerLoadCustomerList(string sContactEmail);

		[OperationContract]
		BrokerCustomersActionResult BrokerLoadCustomersByID(int nBrokerID);

		[OperationContract]
		BrokerPropertiesActionResult BrokerLoadOwnProperties(string sContactEmail);

		[OperationContract]
		BrokerPropertiesActionResult BrokerLoadPropertiesByID(int nBrokerID);

		[OperationContract]
		StringListActionResult BrokerLoadSignedTerms(string sContactEmail);

		[OperationContract]
		BrokerStaticDataActionResult BrokerLoadStaticData(bool bLoadFilesOnly, int originID);

		[OperationContract]
		BrokerPropertiesActionResult BrokerLogin(
			string sEmail,
			Password oPassword,
			string promotionName,
			DateTime? promotionPageVisitTime,
			int uiOriginID
		);

		[OperationContract]
		ActionMetaData BrokerRestorePassword(string sMobile, string sCode);

		[OperationContract]
		StringActionResult BrokerSaveCrmEntry(
			string sType,
			int nActionID,
			int nStatusID,
			string sComment,
			string sCustomerRefNum,
			string sContactEmail
		);

		[OperationContract]
		ActionMetaData BrokerSaveUploadedCustomerFile(
			string sCustomerRefNum,
			string sContactEmail,
			byte[] oFileContents,
			string sFileName
		);

		[OperationContract]
		BrokerPropertiesActionResult BrokerSignup(
			string sFirmName,
			string sFirmRegNum,
			string sContactName,
			string sContactEmail,
			string sContactMobile,
			string sMobileCode,
			string sContactOtherPhone,
			decimal nEstimatedMonthlyClientAmount,
			Password oPassword,
			string sFirmWebSiteUrl,
			int nEstimatedMonthlyApplicationCount,
			bool bIsCaptchEnabled,
			int nBrokerTermsID,
			string sReferredBy,
			bool bFCARegistered,
			string sLicenseNumber,
			int uiOriginID
		);

		[OperationContract]
		ActionMetaData BrokerTransferCommission();

		[OperationContract]
		ActionMetaData BrokerUpdatePassword(string sContactEmail, Password oOldPassword, Password oNewPassword);

		[OperationContract]
		ActionMetaData ChangeBrokerEmail(string oldEmail, string newEmail, string newPassword);

		[OperationContract]
		BoolActionResult IsBroker(string sContactEmail, int uiOrigin);

		[OperationContract]
		StringActionResult BrokerUpdateEmail(int underwriterID, int brokerID, string newEmail);
	} // interface IEzServiceBroker
} // namespace
