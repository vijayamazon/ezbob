﻿namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ActionResults;
	using Ezbob.Backend.Strategies.Broker;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;

	partial class EzServiceImplementation {

		public ActionMetaData BrokerLeadAcquireCustomer(int nCustomerID, int nLeadID, string sFirstName, bool bBrokerFillsForCustomer, string sEmailConfirmationLink) {
			return Execute<BrokerLeadAcquireCustomer>(nCustomerID, null, nCustomerID, nLeadID, sFirstName, bBrokerFillsForCustomer, sEmailConfirmationLink);
		} // BrokerLeadAcquireCustomer

		public ActionMetaData BrokerCustomerWizardComplete(int nCustomerID) {
			return Execute<BrokerCustomerWizardComplete>(nCustomerID, null, nCustomerID);
		} // BrokerCustomerWizardComplete

		public ActionMetaData BrokerCheckCustomerRelevance(int nCustomerID, string sCustomerEmail, bool isAlibaba, string sSourceRef, string sConfirmEmailLink) {
			return Execute<BrokerCheckCustomerRelevance>(nCustomerID, nCustomerID, nCustomerID, sCustomerEmail, isAlibaba, sSourceRef, sConfirmEmailLink);
		} // BrokerCheckCustomerRelevance

		public ActionMetaData BrokerAcceptTerms(int nTermsID, string sContactEmail) {
			return Execute<BrokerAcceptTerms>(null, null, nTermsID, sContactEmail);
		} // BrokerAcceptTerms

		public ActionMetaData BrokerApproveAndResetCustomerPassword(int nUnderwriterID, int nCustomerID, decimal nLoanAmount, int nValidHours, bool isFirst) {
			return Execute(new ExecuteArguments(nCustomerID, nLoanAmount, nValidHours, isFirst) {
				StrategyType = typeof(ApprovedUser),
				CustomerID = nCustomerID,
				UserID = nUnderwriterID,
				OnInit = (stra, amd) => { ((ApprovedUser)stra).SendToCustomer = false; },
			});
		} // BrokerApproveAndResetCustomerPassword

		public BoolActionResult IsBroker(string sContactEmail) {
			BrokerIsBroker oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, sContactEmail);

			return new BoolActionResult {
				MetaData = oResult,
				Value = oInstance.IsBroker,
			};
		} // IsBroker

		public BrokerPropertiesActionResult BrokerSignup(
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
			string sReferredBy
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
				oPassword,
				sFirmWebSiteUrl,
				nEstimatedMonthlyApplicationCount,
				bIsCaptchEnabled,
				nBrokerTermsID,
				sReferredBy
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

		public BrokerPropertiesActionResult BrokerLogin(
			string sEmail,
			Password oPassword,
			string promotionName,
			DateTime? promotionPageVisitTime
		) {
			BrokerLogin oInstance;
			ActionMetaData oMetaData = ExecuteSync(
				out oInstance,
				null,
				null,
				sEmail,
				oPassword,
				promotionName,
				promotionPageVisitTime
			);

			return new BrokerPropertiesActionResult {
				MetaData = oMetaData,
				Properties = oInstance.Properties,
			};
		} // BrokerLogin

		public ActionMetaData BrokerRestorePassword(string sMobile, string sCode) {
			return ExecuteSync<BrokerRestorePassword>(null, null, sMobile, sCode);
		} // BrokerRestorePassword

		public BrokerCustomersActionResult BrokerLoadCustomersByID(int nBrokerID) {
			BrokerLoadCustomerList oIntstance;

			ActionMetaData oResult = ExecuteSync(out oIntstance, null, null, string.Empty, nBrokerID);

			return new BrokerCustomersActionResult {
				MetaData = oResult,
				Customers = oIntstance.Customers,
			};
		} // BrokerLoadCustomersByID

		public BrokerCustomersActionResult BrokerLoadCustomerList(string sContactEmail) {
			BrokerLoadCustomerList oIntstance;

			ActionMetaData oResult = ExecuteSync(out oIntstance, null, null, sContactEmail, 0);

			return new BrokerCustomersActionResult {
				MetaData = oResult,
				Customers = oIntstance.Customers,
			};
		} // BrokerLoadCustomerList

		public BrokerCustomerDetailsActionResult BrokerLoadCustomerDetails(string sCustomerRefNum, string sContactEmail) {
			BrokerLoadCustomerDetails oIntstance;

			ActionMetaData oResult = ExecuteSync(out oIntstance, null, null, sCustomerRefNum, sContactEmail);

			return new BrokerCustomerDetailsActionResult {
				MetaData = oResult,
				Data = oIntstance.Result,
				PotentialSigners = oIntstance.PotentialEsigners,
			};
		} // BrokerLoadCustomerDetails

		public StringActionResult BrokerSaveCrmEntry(string sType, int nActionID, int nStatusID, string sComment, string sCustomerRefNum, string sContactEmail) {
			BrokerSaveCrmEntry oInstance;

			ActionMetaData oResult = ExecuteSync(
				out oInstance,
				null,
				null,
				sType,
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

		public ActionMetaData BrokerSaveUploadedCustomerFile(string sCustomerRefNum, string sContactEmail, byte[] oFileContents, string sFileName) {
			return ExecuteSync<BrokerSaveUploadedCustomerFile>(null, null, sCustomerRefNum, sContactEmail, oFileContents, sFileName);
		} // BrokerSaveUploadedCustomerFile

		public ActionMetaData BrokerDeleteCustomerFiles(string sCustomerRefNum, string sContactEmail, int[] aryFileIDs) {
			return ExecuteSync<BrokerDeleteCustomerFiles>(null, null, sCustomerRefNum, sContactEmail, aryFileIDs);
		} // BrokerDeleteCustomerFiles

		public ActionMetaData BrokerAddCustomerLead(string sLeadFirstName, string sLeadLastName, string sLeadEmail, string sLeadAddMode, string sContactEmail) {
			return ExecuteSync<BrokerAddCustomerLead>(null, null, sLeadFirstName, sLeadLastName, sLeadEmail, sLeadAddMode, sContactEmail);
		} // BrokerAddCustomerLead

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

		public StringActionResult BrokerBackFromCustomerWizard(int nLeadID) {
			BrokerBackFromCustomerWizard oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, nLeadID);

			return new StringActionResult {
				Value = oInstance.ContactEmail,
				MetaData = oResult,
			};
		} // BrokerBackFromCustomerWizard

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

		public BrokerPropertiesActionResult BrokerLoadOwnProperties(string sContactEmail) {
			BrokerLoadOwnProperties oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sContactEmail, 0);

			return new BrokerPropertiesActionResult {
				MetaData = oMetaData,
				Properties = oInstance.Properties,
			};
		} // BrokerLoadOwnProperties

		public BrokerPropertiesActionResult BrokerLoadPropertiesByID(int nBrokerID) {
			BrokerLoadOwnProperties oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, string.Empty, nBrokerID);

			return new BrokerPropertiesActionResult {
				MetaData = oMetaData,
				Properties = oInstance.Properties,
			};
		} // BrokerLoadPropertiesByID

		public ActionMetaData BrokerUpdatePassword(string sContactEmail, Password oOldPassword, Password oNewPassword) {
			BrokerUpdatePassword oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sContactEmail, oOldPassword, oNewPassword);

			if ((oInstance != null) && (oInstance.BrokerID > 0))
				Execute<BrokerPasswordChanged>(null, oInstance.BrokerID, oInstance.BrokerID, oNewPassword);

			return oMetaData;
		} // BrokerUpdatePassword

		public BrokerStaticDataActionResult BrokerLoadStaticData(bool bLoadFilesOnly) {
			var oResult = new BrokerStaticDataActionResult {
				MaxPerNumber = 3,
				MaxPerPage = 10,
				Files = new FileDescription[0],
				Terms = "",
				TermsID = 0,
			};

			try {
				BrokerLoadMarketingFiles oInstance;

				ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null);

				if (oMetaData.Status == ActionStatus.Done)
					oResult.Files = oInstance.Files.ToArray();
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to retrieve marketing files.");
			} // try

			if (bLoadFilesOnly)
				return oResult;

			try {
				BrokerLoadCurrentTerms oInstance;

				ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null);

				if (oMetaData.Status == ActionStatus.Done) {
					oResult.Terms = oInstance.Terms;
					oResult.TermsID = oInstance.ID;
				} // if
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to retrieve terms.");
			} // try

			try {
				BrokerLoadSmsCount oInstance;

				ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null);

				if (oMetaData.Status == ActionStatus.Done) {
					oResult.MaxPerNumber = oInstance.MaxPerNumber;
					oResult.MaxPerPage = oInstance.MaxPerPage;
				} // if
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to retrieve SMS counts.");
			} // try

			try {
				CrmLoadLookups oInstance;

				ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null);

				if (oMetaData.Status == ActionStatus.Done) {
					oResult.Crm = new CrmStaticModel {
						CrmActions = oInstance.Actions,
						CrmRanks = oInstance.Ranks,
						CrmStatuses = oInstance.Statuses.Where(s => !s.IsBroker.HasValue || !s.IsBroker.Value).ToList(),
					};
				} // if
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to retrieve SMS counts.");
			} // try

			return oResult;
		} // BrokerLoadStaticData

		public ActionMetaData BrokerAttachCustomer(int nCustomerID, int? nBrokerID, int nUnderwriterID) {
			return ExecuteSync<BrokerAttachCustomer>(nCustomerID, nUnderwriterID, nCustomerID, nBrokerID, nUnderwriterID);
		} // BrokerAttachCustomer

		public StringListActionResult BrokerLoadSignedTerms(string sContactEmail) {
			BrokerLoadSignedTerms oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sContactEmail);

			return new StringListActionResult {
				MetaData = oMetaData,
				Records = new List<string> { oInstance.Terms, oInstance.SignedTime },
			};
		} // BrokerLoadSignedTerms

		public BrokerInstantOfferResponseActionResult BrokerInstantOffer(BrokerInstantOfferRequest request) {
			BrokerInstantOffer oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, request.BrokerId, request);

			return new BrokerInstantOfferResponseActionResult {
				MetaData = oMetaData,
				Response = oInstance.Response,
			};
		}

	} // class EzServiceImplementation
} // namespace EzService
