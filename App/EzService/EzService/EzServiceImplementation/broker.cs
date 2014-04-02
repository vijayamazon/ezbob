﻿namespace EzService.EzServiceImplementation {
	using System.Linq;
	using EzBob.Backend.Strategies.Broker;
	using EzBob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		#region async

		#region method BrokerLeadAcquireCustomer

		public ActionMetaData BrokerLeadAcquireCustomer(int nCustomerID, int nLeadID, string sEmailConfirmationLink) {
			return Execute<BrokerLeadAcquireCustomer>(nCustomerID, null, nCustomerID, nLeadID, sEmailConfirmationLink);
		} // BrokerLeadAcquireCustomer

		#endregion method BrokerLeadAcquireCustomer

		#region method BrokerCustomerWizardComplete

		public ActionMetaData BrokerCustomerWizardComplete(int nCustomerID) {
			return Execute<BrokerCustomerWizardComplete>(nCustomerID, null, nCustomerID);
		} // BrokerCustomerWizardComplete

		#endregion method BrokerCustomerWizardComplete

		#region method BrokerCheckCustomerRelevance

		public ActionMetaData BrokerCheckCustomerRelevance(int nCustomerID, string sCustomerEmail, string sSourceRef, string sConfirmEmailLink) {
			return Execute<BrokerCheckCustomerRelevance>(nCustomerID, nCustomerID, nCustomerID, sCustomerEmail, sSourceRef, sConfirmEmailLink);
		} // BrokerCheckCustomerRelevance

		#endregion method BrokerCheckCustomerRelevance

		#endregion async

		#region sync

		#region method BrokerLoadSmsCount

		public BrokerSmsCountActionResult BrokerLoadSmsCount() {
			BrokerLoadSmsCount oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null);

			return new BrokerSmsCountActionResult {
				MetaData = oMetaData,
				MaxPerNumber = oInstance.MaxPerNumber,
				MaxPerPage = oInstance.MaxPerPage,
			};
		} // BrokerLoadSmsCount

		#endregion method BrokerLoadSmsCount

		#region method IsBroker

		public BoolActionResult IsBroker(string sContactEmail) {
			BrokerIsBroker oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, sContactEmail);

			return new BoolActionResult {
				MetaData = oResult,
				Value = oInstance.IsBroker,
			};
		} // IsBroker

		#endregion method IsBroker

		#region method BrokerSignup

		public BrokerPropertiesActionResult BrokerSignup(
			string sFirmName,
			string sFirmRegNum,
			string sContactName,
			string sContactEmail,
			string sContactMobile,
			string sMobileCode,
			string sContactOtherPhone,
			decimal nEstimatedMonthlyClientAmount,
			string sPassword,
			string sPassword2,
			string sFirmWebSiteUrl,
			int nEstimatedMonthlyApplicationCount,
			bool bIsCaptchEnabled,
			int nBrokerTermsID
		) {
			BrokerSignup oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null,
				sFirmName,
				sFirmRegNum,
				sContactName,
				sContactEmail,
				sContactMobile,
				sMobileCode,
				sContactOtherPhone,
				nEstimatedMonthlyClientAmount,
				sPassword,
				sPassword2,
				sFirmWebSiteUrl,
				nEstimatedMonthlyApplicationCount,
				bIsCaptchEnabled,
				nBrokerTermsID
			);

			if (oInstance != null) {
				if (oInstance.Properties.BrokerID > 0)
					Execute<BrokerGreeting>(null, oInstance.Properties.BrokerID, oInstance.Properties.BrokerID);

				return new BrokerPropertiesActionResult {
					MetaData = oMetaData,
					Properties = oInstance.Properties,
				};
			} // if

			return new BrokerPropertiesActionResult {
				MetaData = oMetaData,
				Properties = new BrokerProperties(),
			};
		} // BrokerSignup

		#endregion method BrokerSignup

		#region method BrokerLogin

		public BrokerPropertiesActionResult BrokerLogin(string sEmail, string sPassword) {
			BrokerLogin oInstance;
			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sEmail, sPassword);

			return new BrokerPropertiesActionResult {
				MetaData = oMetaData,
				Properties = oInstance.Properties,
			};
		} // BrokerLogin

		#endregion method BrokerLogin

		#region method BrokerRestorePassword

		public ActionMetaData BrokerRestorePassword(string sMobile, string sCode) {
			return ExecuteSync<BrokerRestorePassword>(null, null, sMobile, sCode);
		} // BrokerRestorePassword

		#endregion method BrokerRestorePassword

		#region method BrokerLoadCustomerList

		public BrokerCustomersActionResult BrokerLoadCustomerList(string sContactEmail) {
			BrokerLoadCustomerList oIntstance;

			ActionMetaData oResult = ExecuteSync(out oIntstance, null, null, sContactEmail);

			return new BrokerCustomersActionResult {
				MetaData = oResult,
				Customers = oIntstance.Customers,
			};
		} // BrokerRestorePassword

		#endregion method BrokerLoadCustomerList

		#region method BrokerLoadCustomerDetails

		public BrokerCustomerDetailsActionResult BrokerLoadCustomerDetails(string sCustomerRefNum, string sContactEmail) {
			BrokerLoadCustomerDetails oIntstance;

			ActionMetaData oResult = ExecuteSync(out oIntstance, null, null, sCustomerRefNum, sContactEmail);

			return new BrokerCustomerDetailsActionResult {
				MetaData = oResult,
				Data = oIntstance.Result,
			};
		} // BrokerLoadCustomerDetails

		#endregion method BrokerLoadCustomerDetails

		#region method BrokerSaveCrmEntry

		public StringActionResult BrokerSaveCrmEntry(bool bIsIncoming, int nActionID, int nStatusID, string sComment, string sCustomerRefNum, string sContactEmail) {
			BrokerSaveCrmEntry oInstance;

			ActionMetaData oResult = ExecuteSync(
				out oInstance,
				null,
				null,
				bIsIncoming,
				nActionID,
				nStatusID,
				sComment,
				sCustomerRefNum,
				sContactEmail
			);

			return new StringActionResult {
				MetaData = oResult,
				Value = oInstance.ErrorMsg,
			};
		} // BrokerSaveCrmEntry

		#endregion method BrokerSaveCrmEntry

		#region method BrokerLoadCustomerFiles

		public BrokerCustomerFilesActionResult BrokerLoadCustomerFiles(string sCustomerRefNum, string sContactEmail) {
			BrokerLoadCustomerFiles oInstance;

			ActionMetaData oResult = ExecuteSync(
				out oInstance,
				null,
				null,
				sCustomerRefNum,
				sContactEmail
			);

			return new BrokerCustomerFilesActionResult {
				MetaData = oResult,
				Files = oInstance.Files,
			};
		} // BrokerLoadCustomerFiles

		#endregion method BrokerLoadCustomerFiles

		#region method BrokerDownloadCustomerFile

		public BrokerCustomerFileContentsActionResult BrokerDownloadCustomerFile(string sCustomerRefNum, string sContactEmail, int nFileID) {
			BrokerDownloadCustomerFile oInstance;

			ActionMetaData oResult = ExecuteSync(
				out oInstance,
				null,
				null,
				sCustomerRefNum,
				sContactEmail,
				nFileID
			);

			return new BrokerCustomerFileContentsActionResult {
				MetaData = oResult,
				Name = oInstance.FileName,
				Contents = oInstance.Contents,
			};
		} // BrokerLoadCustomerFiles

		#endregion method BrokerDownloadCustomerFile

		#region method BrokerSaveUploadedCustomerFile

		public ActionMetaData BrokerSaveUploadedCustomerFile(string sCustomerRefNum, string sContactEmail, byte[] oFileContents, string sFileName) {
			return ExecuteSync<BrokerSaveUploadedCustomerFile>(null, null, sCustomerRefNum, sContactEmail, oFileContents, sFileName);
		} // BrokerSaveUploadedCustomerFile

		#endregion method BrokerSaveUploadedCustomerFile

		#region method BrokerDeleteCustomerFiles

		public ActionMetaData BrokerDeleteCustomerFiles(string sCustomerRefNum, string sContactEmail, int[] aryFileIDs) {
			return ExecuteSync<BrokerDeleteCustomerFiles>(null, null, sCustomerRefNum, sContactEmail, aryFileIDs);
		} // BrokerDeleteCustomerFiles

		#endregion method BrokerDeleteCustomerFiles

		#region method BrokerAddCustomerLead

		public ActionMetaData BrokerAddCustomerLead(string sLeadFirstName, string sLeadLastName, string sLeadEmail, string sLeadAddMode, string sContactEmail) {
			return ExecuteSync<BrokerAddCustomerLead>(null, null, sLeadFirstName, sLeadLastName, sLeadEmail, sLeadAddMode, sContactEmail);
		} // BrokerAddCustomerLead

		#endregion method BrokerAddCustomerLead

		#region method BrokerLeadCanFillWizard

		public BrokerLeadDetailsActionResult BrokerLeadCanFillWizard(int nLeadID, string sLeadEmail, string sContactEmail) {
			BrokerLeadCanFillWizard oInstance;

			ActionMetaData oResult = ExecuteSync(
				out oInstance,
				null,
				null,
				nLeadID,
				sLeadEmail,
				sContactEmail
			);

			return new BrokerLeadDetailsActionResult {
				LeadID = oInstance.LeadID,
				CustomerID = oInstance.CustomerID,
				LeadEmail = oInstance.LeadEmail,
				FirstName = oInstance.FirstName,
				LastName = oInstance.LastName,
				MetaData = oResult,
			};
		} // BrokerLeadCanFillWizard

		#endregion method BrokerLeadCanFillWizard

		#region method BrokerBackFromCustomerWizard

		public StringActionResult BrokerBackFromCustomerWizard(int nLeadID) {
			BrokerBackFromCustomerWizard oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, nLeadID);

			return new StringActionResult {
				Value = oInstance.ContactEmail,
				MetaData = oResult,
			};
		} // BrokerBackFromCustomerWizard

		#endregion method BrokerBackFromCustomerWizard

		#region method BrokerLeadCheckToken

		public BrokerLeadDetailsActionResult BrokerLeadCheckToken(string sToken) {
			BrokerLeadCheckToken oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, sToken);

			return new BrokerLeadDetailsActionResult {
				LeadID = oInstance.LeadID,
				CustomerID = oInstance.CustomerID,
				LeadEmail = oInstance.LeadEmail,
				FirstName = oInstance.FirstName,
				LastName = oInstance.LastName,
				MetaData = oResult,
			};
		} // BrokerLeadCheckToken

		#endregion method BrokerLeadCheckToken

		#region method BrokerLoadOwnProperties

		public BrokerPropertiesActionResult BrokerLoadOwnProperties(string sContactEmail) {
			BrokerLoadOwnProperties oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sContactEmail);

			return new BrokerPropertiesActionResult {
				MetaData = oMetaData,
				Properties = oInstance.Properties,
			};
		} // BrokerLoadOwnProperties

		#endregion method BrokerLoadOwnProperties

		#region method BrokerLoadCurrentTerms

		public BrokerTermsActionResult BrokerLoadCurrentTerms() {
			BrokerLoadCurrentTerms oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null);

			return new BrokerTermsActionResult {
				MetaData = oMetaData,
				Terms = oInstance.Terms,
				TermsID = oInstance.ID,
			};
		} // BrokerLoadCurrentTerms

		#endregion method BrokerLoadCurrentTerms

		#endregion sync
	} // class EzServiceImplementation
} // namespace EzService
