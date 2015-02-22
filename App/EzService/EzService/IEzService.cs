namespace EzService {
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using EchoSignLib;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.PricingModel;
	using Ezbob.Backend.Strategies.UserManagement;
	using EzBob.Backend.Models;
	using EzService.ActionResults;
	using EZBob.DatabaseLib.Model.Database;
	using SalesForceLib.Models;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzService {
		[OperationContract]
		ActionMetaData AddCciHistory(int nCustomerID, int nUnderwriterID, bool bCciMark);

		[OperationContract]
		ActionMetaData AndRecalculateVatReturnSummaryForAll();

		[OperationContract]
		ActionMetaData ApprovedUser(int userId, int customerId, decimal loanAmount, int nValidHours, bool isFirst);

		[OperationContract]
		ActionMetaData BackfillAml();

		[OperationContract]
		ActionMetaData BackfillCustomerAnalyticsCompany();

		[OperationContract]
		ActionMetaData BackfillExperianConsumer();

		[OperationContract]
		ActionMetaData BackfillExperianDirectors(int? nCustomerID);

		[OperationContract]
		ActionMetaData BackfillExperianLtd();

		[OperationContract]
		ActionMetaData BackfillHmrcBusinessRelevance();

		[OperationContract]
		ActionMetaData BackfillLandRegistry2PropertyLink();

		[OperationContract]
		ActionMetaData BackfillNonLimitedCompanies();

		[OperationContract]
		ActionMetaData BackfillTurnover();

		[OperationContract]
		ActionMetaData BackfillZooplaValue();

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
		BrokerStaticDataActionResult BrokerLoadStaticData(bool bLoadFilesOnly);

		[OperationContract]
		BrokerPropertiesActionResult BrokerLogin(
			string sEmail,
			Password oPassword,
			string promotionName,
			DateTime? promotionPageVisitTime
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
			string sLicenseNumber
			);

		[OperationContract]
		ActionMetaData BrokerUpdatePassword(string sContactEmail, Password oOldPassword, Password oNewPassword);

		[OperationContract]
		ActionMetaData CaisGenerate(int underwriterId);

		[OperationContract]
		ActionMetaData CaisUpdate(int userId, int caisId);

		[OperationContract]
		ActionMetaData CalculateMedal(int underwriterId, int customerId);

		[OperationContract]
		MarketplacesActionResult CalculateModelsAndAffordability(int userId, int nCustomerID, DateTime? oHistory);

		[OperationContract]
		ActionMetaData CalculateOffer(
			int underwriterId,
			int customerId,
			int amount,
			bool hasLoans,
			Medal medalClassification
			);

		[OperationContract]
		ActionMetaData CalculateVatReturnSummary(int nCustomerMarketplaceID);

		[OperationContract]
		ActionMetaData CashTransferred(int customerId, decimal amount, string loanRefNum, bool isFirst);

		[OperationContract]
		ActionMetaData ChangeBrokerEmail(string oldEmail, string newEmail, string newPassword);

		[OperationContract]
		ActionMetaData CheckAml(int customerId, int userId);

		[OperationContract]
		ActionMetaData CheckAmlCustom(
			int userId,
			int customerId,
			string idhubHouseNumber,
			string idhubHouseName,
			string idhubStreet,
			string idhubDistrict,
			string idhubTown,
			string idhubCounty,
			string idhubPostCode
			);

		[OperationContract]
		ActionMetaData CheckBwa(int customerId, int userId);

		[OperationContract]
		ActionMetaData CheckBwaCustom(
			int userId,
			int customerId,
			string idhubHouseNumber,
			string idhubHouseName,
			string idhubStreet,
			string idhubDistrict,
			string idhubTown,
			string idhubCounty,
			string idhubPostCode,
			string idhubBranchCode,
			string idhubAccountNumber
			);

		[OperationContract]
		ExperianLtdActionResult CheckLtdCompanyCache(int userId, string sCompanyRefNum);

		[OperationContract]
		ActionMetaData CompanyFilesUpload(int customerId, string fileName, byte[] fileContent, string fileContentType);

		[OperationContract]
		CrmLookupsActionResult CrmLoadLookups();

		[OperationContract]
		StringActionResult CustomerChangePassword(string sEmail, Password oOldPassword, Password oNewPassword);

		[OperationContract]
		UserLoginActionResult CustomerSignup(
			string sEmail,
			Password oPassword,
			int nPasswordQuestion,
			string sPasswordAnswer,
			string sRemoteIp
		);

		[OperationContract]
		ActionMetaData DeleteExperianDirector(int nDirectorID, int nUnderwriterID);

		[OperationContract]
		ActionMetaData DisableCurrentManualPacnetDeposits(int underwriterId);

		[OperationContract]
		ActionMetaData DisplayMarketplaceSecurityData(int nCustomerID);

		[OperationContract]
		IntActionResult EmailConfirmationCheckOne(Guid oToken);

		[OperationContract]
		ActionMetaData EmailConfirmationConfirmUser(int nUserID, int nUnderwriterID);

		[OperationContract]
		EmailConfirmationTokenActionResult EmailConfirmationGenerate(int nUserID);

		[OperationContract]
		ActionMetaData EmailConfirmationGenerateAndSend(int nUserID, int underwriterId);

		[OperationContract]
		ActionMetaData EmailHmrcParsingErrors(
			int nCustomerID,
			int nCustomerMarketplaceID,
			SortedDictionary<string, string> oErrorsToEmail
			);

		[OperationContract]
		ActionMetaData EmailRolloverAdded(int userId, int customerId, decimal amount);

		[OperationContract]
		ActionMetaData EmailUnderReview(int customerId);

		[OperationContract]
		ActionMetaData EncryptChannelGrabberMarketplaces();

		[OperationContract]
		ActionMetaData Escalated(int customerId, int userId);

		[OperationContract]
		ActionMetaData EsignProcessPending(int? nCustomerID);

		[OperationContract]
		StringActionResult EsignSend(int userId, EchoSignEnvelope[] oPackage);

		[OperationContract]
		ActionMetaData ExperianCompanyCheck(int userId, int customerId, bool forceCheck);

		[OperationContract]
		ActionMetaData ExperianConsumerCheck(int userId, int nCustomerID, int? nDirectorID, bool bForceCheck);

		[OperationContract]
		AccountsToUpdateActionResult FindAccountsToUpdate(int nCustomerID);

		[OperationContract]
		ActionMetaData FinishWizard(FinishWizardArgs oArgs, int underwriterId);

		[OperationContract]
		ActionMetaData FirstOfMonthStatusNotifier();

		[OperationContract]
		ActionMetaData FraudChecker(int customerId, FraudMode mode);

		[OperationContract]
		BoolActionResult GenerateMobileCode(string phone);

		[OperationContract]
		AvailableFundsActionResult GetAvailableFunds(int underwriterId);

		[OperationContract]
		ActionMetaData GetCashFailed(int customerId);

		[OperationContract]
		CompanyCaisDataActionResult GetCompanyCaisDataForAlerts(int underwriterId, int customerId);

		[OperationContract]
		CompanyDataForCompanyScoreActionResult GetCompanyDataForCompanyScore(int underwriterId, string refNumber);

		[OperationContract]
		CompanyDataForCreditBureauActionResult GetCompanyDataForCreditBureau(int underwriterId, string refNumber);

		[OperationContract]
		byte[] GetCompanyFile(int userId, int companyFileId);

		[OperationContract]
		NullableDateTimeActionResult GetCompanySeniority(int customerId, bool isLimited, int underwriterId); // TODO: remove

		[OperationContract]
		ConfigTableActionResult GetConfigTable(int nUnderwriterID, string sTableName);

		[OperationContract]
		DecimalActionResult GetCurrentCustomerAnnualTurnover(int customerID);

		[OperationContract]
		CustomerManualAnnualizedRevenueActionResult GetCustomerManualAnnualizedRevenue(int nCustomerID);

		[OperationContract]
		StringActionResult GetCustomerState(int customerId);

		[OperationContract]
		IntActionResult GetExperianAccountsCurrentBalance(int customerId, int underwriterId);

		[OperationContract]
		DateTimeActionResult GetExperianCompanyCacheDate(int userId, string refNumber);

		[OperationContract]
		IntActionResult GetExperianConsumerScore(int customerId);

		[OperationContract]
		DecimalActionResult GetPricingModelDefaultRate(int customerId, int underwriterId, decimal companyShare);

		[OperationContract]
		PricingModelModelActionResult GetPricingModelModel(int customerId, int underwriterId, string scenarioName);

		[OperationContract]
		StringListActionResult GetPricingModelScenarios(int underwriterId);

		[OperationContract]
		PropertyStatusesActionResult GetPropertyStatuses();

		[OperationContract]
		WizardConfigsActionResult GetWizardConfigs();

		[OperationContract]
		ActionMetaData GetZooplaData(int customerId, bool reCheck);

		[OperationContract]
		BoolActionResult IsBroker(string sContactEmail);

		[OperationContract]
		string LandRegistryEnquiry(
			int userId,
			int customerId,
			string buildingNumber,
			string buildingName,
			string streetName,
			string cityName,
			string postCode
			);

		[OperationContract]
		string LandRegistryRes(int userId, int customerId, string titleNumber);

		[OperationContract]
		ActionMetaData LateBy14Days();

		[OperationContract]
		CustomerDetailsActionResult LoadCustomerByCreatePasswordToken(Guid oToken);

		[OperationContract]
		StringStringMapActionResult LoadCustomerLeadFieldNames();

		[OperationContract]
		EsignatureFileActionResult LoadEsignatureFile(int userId, long nEsignatureID);

		[OperationContract]
		EsignatureListActionResult LoadEsignatures(int userId, int? nCustomerID, bool bPollStatus);

		[OperationContract]
		ExperianConsumerActionResult LoadExperianConsumer(int userId, int customerId, int? directorId, long? nServiceLogId);

		[OperationContract]
		ExperianConsumerMortgageActionResult LoadExperianConsumerMortgageData(int userId, int customerId);

		[OperationContract]
		ExperianLtdActionResult LoadExperianLtd(long nServiceLogID);

		[OperationContract]
		VatReturnPeriodsActionResult LoadManualVatReturnPeriods(int nCustomerID);

		[OperationContract]
		VatReturnDataActionResult LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID);

		[OperationContract]
		VatReturnDataActionResult LoadVatReturnRawData(int nCustomerMarketplaceID);

		[OperationContract]
		VatReturnDataActionResult LoadVatReturnSummary(int nCustomerID, int nMarketplaceID);

		[OperationContract]
		ActionMetaData LoanFullyPaid(int customerId, string loanRefNum);

		[OperationContract]
		ActionMetaData MainStrategy1(
			int uderwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison
			);

		[OperationContract]
		ActionMetaData MainStrategySync1(
			int underwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison
			);

		[OperationContract]
		ActionMetaData MarketplaceInstantUpdate(int nMarketplaceID);

		[OperationContract]
		ActionMetaData MarkSessionEnded(int nSessionID, string sComment, int? nCustomerId);

		[OperationContract]
		ActionMetaData MoreAmlAndBwaInformation(int userId, int customerId);

		[OperationContract]
		ActionMetaData MoreAmlInformation(int userId, int customerId);

		[OperationContract]
		ActionMetaData MoreBwaInformation(int userId, int customerId);

		[OperationContract]
		ActionMetaData NotifySalesOnNewCustomer(int nCustomerID);

		[OperationContract]
		ExperianConsumerActionResult ParseExperianConsumer(long nServiceLogId);

		[OperationContract]
		ExperianLtdActionResult ParseExperianLtd(long nServiceLogID);

		[OperationContract]
		ActionMetaData PasswordRestored(int customerId);

		[OperationContract]
		ActionMetaData PayEarly(int customerId, decimal amount, string loanRefNum);

		[OperationContract]
		ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId);

		[OperationContract]
		ActionMetaData PayPointCharger();

		[OperationContract]
		ActionMetaData PayPointNameValidationFailed(int userId, int customerId, string cardHolderName);

		[OperationContract]
		ActionMetaData PostcodeSaveLog(
			string sRequestType,
			string sUrl,
			string sStatus,
			string sResponseData,
			string sErrorMessage,
			int nUserID
			);

		[OperationContract]
		PricingModelModelActionResult PricingModelCalculate(int customerId, int underwriterId, PricingModelModel model);

		[OperationContract]
		QuickOfferActionResult QuickOffer(int customerId, bool saveOfferToDB);

		[OperationContract]
		QuickOfferActionResult QuickOfferWithPrerequisites(int customerId, bool saveOfferToDB);

		[OperationContract]
		ActionMetaData RecordManualPacnetDeposit(int underwriterId, string underwriterName, int amount);

		[OperationContract]
		ActionMetaData RejectUser(int userId, int customerId, bool bSendToCustomer);

		[OperationContract]
		ActionMetaData RemoveManualVatReturnPeriod(Guid oPeriodID);

		[OperationContract]
		ActionMetaData RenewEbayToken(int userId, int customerId, string marketplaceName, string eBayAddress);

		[OperationContract]
		ActionMetaData RequestCashWithoutTakenLoan(int customerId);

		[OperationContract]
		ActionMetaData ResetPassword123456(int nUnderwriterID, int nTargetID, PasswordResetTarget nTarget);

		
		[OperationContract]
		ActionMetaData SalesForceAddUpdateLeadAccount(int? userID, string email, int? customerID, bool isBrokerLead, bool isVipLead);

		[OperationContract]
		ActionMetaData SalesForceAddUpdateContact(int? userID, int customerID, int? directorID, string directorEmail);

		[OperationContract]
		ActionMetaData SalesForceAddTask(int? userID, int customerID, TaskModel model);

		[OperationContract]
		ActionMetaData SalesForceAddActivity(int? userID, int customerID, ActivityModel model);

		[OperationContract]
		ActionMetaData SalesForceAddOpportunity(int? userID, int customerID, OpportunityModel model);

		[OperationContract]
		ActionMetaData SalesForceUpdateOpportunity(int? userID, int customerID, OpportunityModel model);

		[OperationContract]
		ActionMetaData SaveAgreement(
			int customerId,
			AgreementModel model,
			string refNumber,
			string name,
			TemplateModel template,
			string path1,
			string path2
			);

		[OperationContract]
		BoolActionResult SaveConfigTable(List<ConfigTable> configTableEntries, ConfigTableType configTableType);

		[OperationContract]
		ActionMetaData SavePricingModelSettings(int underwriterId, string scenarioName, PricingModelModel model);

		[OperationContract]
		ActionMetaData SaveSourceRefHistory(
			int nUserID,
			string sSourceRefList,
			string sVisitTimeList,
			CampaignSourceRef campaignSourceRef
			);

		[OperationContract]
		ElapsedTimeInfoActionResult SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
			);

		[OperationContract]
		ActionMetaData SendPendingMails(int underwriterId, int customerId);

		[OperationContract]
		BoolActionResult SendSms(int userId, int underwriterId, string phone, string content);

		[OperationContract]
		CustomerManualAnnualizedRevenueActionResult SetCustomerManualAnnualizedRevenue(
			int nCustomerID,
			decimal nRevenue,
			string sComment
			);

		[OperationContract]
		IntActionResult SetCustomerPasswordByToken(string sEmail, Password oPassword, Guid oToken, bool bIsBrokerLead);

		[OperationContract]
		ActionMetaData SetLateLoanStatus();

		[OperationContract]
		ActionMetaData Temp_BackFillMedals();

		[OperationContract]
		ActionMetaData TransferCashFailed(int customerId);

		[OperationContract]
		ActionMetaData UnderwriterSignup(string name, Password password, string role);

		[OperationContract]
		ActionMetaData UpdateConfigurationVariables(int userId);

		[OperationContract]
		ActionMetaData UpdateCurrencyRates();

		[OperationContract]
		ActionMetaData UpdateCustomerAnalyticsOnCompanyChange(int customerID);

		[OperationContract]
		ActionMetaData UpdateExperianDirectorDetails(int? nCustomerID, int? nUnderwriterID, Esigner oDetails);

		[OperationContract]
		ActionMetaData UpdateGoogleAnalytics(DateTime? oBackfillStartDate, DateTime? oBackfillEndDate);

		[OperationContract]
		ActionMetaData UpdateLinkedHmrcPassword(string sCustomerID, string sDisplayName, string sPassword, string sHash);

		[OperationContract]
		ActionMetaData UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep, int userId);

		[OperationContract]
		ActionMetaData UpdateTransactionStatus();

		[OperationContract]
		StringActionResult UserChangeEmail(int underwriterId, int nUserID, string sNewEmail);

		[OperationContract]
		StringActionResult UserChangePassword(
			string sEmail,
			Password oOldPassword,
			Password oNewPassword,
			bool bForceChangePassword
		);

		[OperationContract]
		UserLoginActionResult UserLogin(
			string sEmail,
			Password sPassword,
			string sRemoteIp,
			string promotionName,
			DateTime? promotionPageVisitTime
		);

		[OperationContract]
		StringActionResult UserResetPassword(string sEmail);

		[OperationContract]
		StringActionResult UserUpdateSecurityQuestion(string sEmail, Password oPassword, int nQuestionID, string sAnswer);

		[OperationContract]
		StringActionResult ValidateAndUpdateLinkedHmrcPassword(
			string sCustomerID,
			string sDisplayName,
			string sPassword,
			string sHash
			);

		[OperationContract]
		BoolActionResult ValidateMobileCode(string phone, string code);

		[OperationContract]
		ActionMetaData MaamMedalAndPricing(int nCustomerCount, int nLastCheckedCashRequestID);

		[OperationContract]
		ActionMetaData VerifyMedal(int topCount, int lastCheckedID, bool includeTest, DateTime? calculationTime);

		[OperationContract]
		ActionMetaData VerifyApproval(int nCustomerCount, int nLastCheckedCustomerID);

		[OperationContract]
		ActionMetaData VerifyEnoughAvailableFunds(int underwriterId, decimal deductAmount);

		[OperationContract]
		ActionMetaData VerifyReapproval(int nCustomerCount, int nLastCheckedCustomerID);

		[OperationContract]
		ActionMetaData VerifyReject(int nCustomerCount, int nLastCheckedCustomerID);

		[OperationContract]
		ActionMetaData VerifyRerejection(int nCustomerCount, int nLastCheckedCustomerID);

		[OperationContract]
		ActionMetaData VipRequest(int customerId, string fullname, string email, string phone);

		[OperationContract]
		ActionMetaData XDaysDue();

		[OperationContract]
		ActionMetaData ChangeLotteryPlayerStatus(int customerID, Guid playerID, LotteryPlayerStatus newStatus);

		[OperationContract]
		LotteryActionResult PlayLottery(int customerID, Guid playerID);

		[OperationContract]
		ActionMetaData EnlistLottery(int customerID);
	} // interface IEzService
} // namespace EzService
