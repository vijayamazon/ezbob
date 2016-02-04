namespace EzService {
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using DbConstants;
	using EchoSignLib;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.PricingModel;
	using EzBob.Backend.Models;
	using EzService.ActionResults;
	using EZBob.DatabaseLib.Model.Database;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzService : // Add base interfaces in the following lines and in alphabetic order. Please.
		IEzAutomationVerification,
		IEzServiceBroker,
		IEzServiceInvestor,
		IEzServiceLogicalGlue,
		IEzServiceMainStrategy,
		IEzServiceNewLoan,
		IEzServiceSalesForce,
		IEzServiceVatReturn,
		IEzServiceUserManagement,
        IEzServiceLegalDocs,
        IEzServiceSecurity
	{
		[OperationContract]
		ActionMetaData AddHistoryDirector(Esigner Edirector);

		[OperationContract]
		ActionMetaData ApprovedUser(int userId, int customerId, decimal loanAmount, int nValidHours, bool isFirst);

		[OperationContract]
		ActionMetaData BackfillAml();

		[OperationContract]
		ActionMetaData BackFillExperianNonLtdScoreText();

		[OperationContract]
		ActionMetaData BackfillExperianConsumer();

		[OperationContract]
		ActionMetaData BackfillExperianDirectors(int? nCustomerID);

		[OperationContract]
		ActionMetaData BackfillExperianLtd();

		[OperationContract]
		ActionMetaData BackfillExperianLtdScoreText();

		[OperationContract]
		ActionMetaData BackfillHmrcBusinessRelevance();

		[OperationContract]
		ActionMetaData BackfillLandRegistry2PropertyLink();

		[OperationContract]
		ActionMetaData BackfillMedalForAll();

		[OperationContract]
		ActionMetaData BackfillNonLimitedCompanies();

		[OperationContract]
		ActionMetaData BackfillTurnover();

		[OperationContract]
		ActionMetaData BackfillZooplaValue();

		[OperationContract]
		ActionMetaData BackfillBrokerCommissionInvoice();

		[OperationContract]
		ActionMetaData CaisGenerate(int underwriterId);

		[OperationContract]
		ActionMetaData CaisUpdate(int userId, int caisId);

		[OperationContract]
		ActionMetaData CalculateMedal(int underwriterId, int customerId, long? cashRequestID, long? nlCashRequestID);

		[OperationContract]
		MarketplacesActionResult CalculateModelsAndAffordability(int userId, int nCustomerID, DateTime? oHistory);

		[OperationContract]
		ActionMetaData CashTransferred(int customerId, decimal amount, string loanRefNum, bool isFirst);

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
		ActionMetaData CompanyFilesUpload(int customerId, string fileName, byte[] fileContent, string fileContentType, bool isBankStatement);

		[OperationContract]
		CrmLookupsActionResult CrmLoadLookups();

		[OperationContract]
		ActionMetaData CustomerBankAccountIsAlreadyAddedEmail(int customerID);

		[OperationContract]
		ActionMetaData DeleteExperianDirector(int nDirectorID, int nUnderwriterID);

		[OperationContract]
		ActionMetaData DisableCurrentManualPacnetDeposits(int underwriterId);

		[OperationContract]
		ActionMetaData DisplayMarketplaceSecurityData(int nCustomerID);

		[OperationContract]
		ActionMetaData EmailHmrcParsingErrors(
			int nCustomerID,
			int nCustomerMarketplaceID,
			SortedDictionary<string, string> oErrorsToEmail
		);

		[OperationContract]
		ActionMetaData EmailRolloverAdded(int userId, int customerId, decimal amount);

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
		CollectionSnailMailActionResult GetCollectionSnailMail(int userID, int collectionSnailMailID);

		[OperationContract]
		ActionMetaData GetZooplaData(int customerId, bool reCheck);

		[OperationContract]
		ActionMetaData IovationCheck(IovationCheckModel model);

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
		ActionMetaData LoanFullyPaid(int customerId, string loanRefNum);

		[OperationContract]
        ActionMetaData LoanStatusAfterPayment(int userId, int customerID, string customerEmail, int loanID, decimal paymentAmount, bool sendMail, decimal? balance = null, bool? isPaidOff = null);

		[OperationContract]
		ActionMetaData MarketplaceInstantUpdate(int nMarketplaceID);

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
		ActionMetaData PostcodeNuts(int nUserID, string postcode);

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
		ActionMetaData RenewEbayToken(int userId, int customerId, string marketplaceName, string eBayAddress);

		[OperationContract]
		ActionMetaData RequestCashWithoutTakenLoan(int customerId);

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
		ActionMetaData SetLateLoanStatus();

		[OperationContract]
		ActionMetaData Temp_BackFillMedals();

		[OperationContract]
		ActionMetaData TransferCashFailed(int customerId);

		[OperationContract]
		ActionMetaData UpdateConfigurationVariables(int userId);

		[OperationContract]
		ActionMetaData UpdateCurrencyRates();

		[OperationContract]
		ActionMetaData UpdateExperianDirectorDetails(int? nCustomerID, int? nUnderwriterID, Esigner oDetails);

		[OperationContract]
		ActionMetaData UpdateGoogleAnalytics(DateTime? oBackfillStartDate, DateTime? oBackfillEndDate);

		[OperationContract]
		ActionMetaData UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep, int userId);

		[OperationContract]
		ActionMetaData UpdateTransactionStatus();

		[OperationContract]
		BoolActionResult ValidateMobileCode(string phone, string code);

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

		[OperationContract]
		AlibabaAvailableCreditActionResult CustomerAvaliableCredit(string customerRefNum, long aliMemberID);

		[OperationContract]
		ActionMetaData RequalifyCustomer(string customerRefNum, long aliMemberID);

		[OperationContract]
		AlibabaSaleContractActionResult SaleContract(AlibabaContractDto dto);

		[OperationContract]
		ActionMetaData DataSharing(int customerID, AlibabaBusinessType businessType, int? uwID);

		[OperationContract]
		ActionMetaData SaveApiCall(ApiCallData data);

		[OperationContract]
		ActionMetaData VerifyEnoughAvailableFunds(int underwriterId, decimal deductAmount);

		[OperationContract]
		ActionMetaData ParseCreditSafeLtd(int customerID, int userID, long serviceLogID);

		[OperationContract]
		ActionMetaData ParseCreditSafeNonLtd(int customerID, int userID, long serviceLogID);
		[OperationContract]
		ExperianTargetingActionResult ExperianTarget(int customerID, int userID, ExperianTargetingRequest request);

		[OperationContract]
		ActionMetaData WriteToServiceLog(int customerID, int userID, WriteToLogPackage.InputData packageInputData);

		[OperationContract]
		ActionMetaData PayPointAddedWithoutOpenLoan(int customerID, int userID, decimal amount, string paypointTransactionID);

		[OperationContract]
		ActionMetaData TopUpDelivery(int underwriterId, decimal amount, int contentCase);

		[OperationContract]
		ActionMetaData PacnetDelivery(int underwriterId, decimal amount);

		[OperationContract]
		ActionMetaData BackfillDailyLoanStats();

		[OperationContract]
		LoanCommissionDefaultsActionResult GetLoanCommissionDefaults(
			int underwriterID,
			long cashRequestID,
			decimal loanAmount
		);

		[OperationContract]
		ActionMetaData GetIncomeSms(DateTime? date, bool isYesterday);

		[OperationContract]
		ApplicationInfoResult LoadApplicationInfo(int? underwriterID, int customerID, long? cashRequestID, DateTime? now);

		[OperationContract]
		StringStringMapActionResult SetManualDecision(DecisionModel model);

		[OperationContract]
		MultiBrandLoanSummaryActionResult BuildMultiBrandLoanSummary(int customerID);

		[OperationContract]
		DecisionHistoryResult LoadDecisionHistory(int customerID, int underwriterID);

		[OperationContract]
		MessagesListActionResult LoadMessagesSentToUser(int userID);

		[OperationContract]
		SlidersDataActionResults GetSlidersData(int customerID);
	} // interface IEzService
} // namespace EzService
