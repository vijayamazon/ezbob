namespace EzService {
	using System.Collections.Generic;
	using System.ServiceModel;
	using ActionResults;
	using EzBob.Backend.Strategies;
	using EzBob.Models.Agreements;
	using EzBob.Web.Areas.Underwriter.Models;
	using FraudChecker;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzService {
		#region Agreements

		[OperationContract]
		ActionMetaData SaveAgreement(int customerId, AgreementModel model, string refNumber, string name, TemplateModel template, string path1, string path2);

		#endregion Agreements

		#region AML and BWA

		[OperationContract]
		ActionMetaData CheckAml(int customerId);

		[OperationContract]
		ActionMetaData CheckAmlCustom(int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet, string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode);

		[OperationContract]
		ActionMetaData CheckBwa(int customerId);

		[OperationContract]
		ActionMetaData CheckBwaCustom(int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet, string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode, string idhubBranchCode, string idhubAccountNumber);

		#endregion AML and BWA

		#region Broker

		[OperationContract]
		BrokerSmsCountActionResult BrokerLoadSmsCount();

		[OperationContract]
		BoolActionResult IsBroker(string sContactEmail);

		[OperationContract]
		ActionMetaData BrokerSignup(
			string FirmName,
			string FirmRegNum,
			string ContactName,
			string ContactEmail,
			string ContactMobile,
			string MobileCode,
			string ContactOtherPhone,
			decimal EstimatedMonthlyClientAmount,
			string Password,
			string Password2,
			string sFirmWebSiteUrl,
			int nEstimatedMonthlyApplicationCount
		);

		[OperationContract]
		ActionMetaData BrokerLogin(string Email, string Password);

		[OperationContract]
		ActionMetaData BrokerRestorePassword(string sMobile, string sCode);

		[OperationContract]
		BrokerCustomersActionResult BrokerLoadCustomerList(string sContactEmail);

		[OperationContract]
		BrokerCustomerDetailsActionResult BrokerLoadCustomerDetails(string sCustomerRefNum, string sContactEmail);

		[OperationContract]
		StringActionResult BrokerSaveCrmEntry(bool bIsIncoming, int nActionID, int nStatusID, string sComment, string sCustomerRefNum, string sContactEmail);

		[OperationContract]
		BrokerCustomerFilesActionResult BrokerLoadCustomerFiles(string sCustomerRefNum, string sContactEmail);

		[OperationContract]
		BrokerCustomerFileContentsActionResult BrokerDownloadCustomerFile(string sCustomerRefNum, string sContactEmail, int nFileID);

		[OperationContract]
		ActionMetaData BrokerSaveUploadedCustomerFile(string sCustomerRefNum, string sContactEmail, byte[] oFileContents, string sFileName);

		[OperationContract]
		ActionMetaData BrokerDeleteCustomerFiles(string sCustomerRefNum, string sContactEmail, int[] aryFileIDs);

		[OperationContract]
		ActionMetaData BrokerAddCustomerLead(string sLeadFirstName, string sLeadLastName, string sLeadEmail, string sLeadAddMode, string sContactEmail);

		[OperationContract]
		BrokerLeadDetailsActionResult BrokerLeadCanFillWizard(int nLeadID, string sLeadEmail, string sContactEmail);

		[OperationContract]
		ActionMetaData BrokerLeadAcquireCustomer(int nCustomerID, int nLeadID, string sEmailConfirmationLink);

		[OperationContract]
		ActionMetaData BrokerCustomerWizardComplete(int nCustomerID);

		[OperationContract]
		StringActionResult BrokerBackFromCustomerWizard(int nLeadID);

		[OperationContract]
		BrokerLeadDetailsActionResult BrokerLeadCheckToken(string sToken);

		[OperationContract]
		ActionMetaData BrokerCheckCustomerRelevance(int nCustomerID, string sCustomerEmail, string sSourceRef, string sConfirmEmailLink);

		#endregion Broker

		#region CAIS

		[OperationContract]
		ActionMetaData CaisGenerate(int underwriterId);

		[OperationContract]
		ActionMetaData CaisUpdate(int userId, int caisId);

		#endregion CAIS

		#region Company Files

		[OperationContract]
		ActionMetaData CompanyFilesUpload(int customerId, string fileName, byte[] fileContent, string fileContentType);

		[OperationContract]
		byte[] GetCompanyFile(int companyFileId);

		#endregion

		#region email strategies

		[OperationContract]
		ActionMetaData GreetingMailStrategy(int nCustomerID, string sConfirmationEmail);

		[OperationContract]
		ActionMetaData ApprovedUser(int userId, int customerId, decimal loanAmount);

		[OperationContract]
		ActionMetaData CashTransferred(int customerId, decimal amount);

		[OperationContract]
		ActionMetaData EmailUnderReview(int customerId);

		[OperationContract]
		ActionMetaData Escalated(int customerId);

		[OperationContract]
		ActionMetaData GetCashFailed(int customerId);

		[OperationContract]
		ActionMetaData LoanFullyPaid(int customerId, string loanRefNum);

		[OperationContract]
		ActionMetaData MoreAmlAndBwaInformation(int userId, int customerId);

		[OperationContract]
		ActionMetaData MoreAmlInformation(int userId, int customerId);

		[OperationContract]
		ActionMetaData MoreBwaInformation(int userId, int customerId);

		[OperationContract]
		ActionMetaData PasswordChanged(int customerId, string password);

		[OperationContract]
		ActionMetaData PasswordRestored(int customerId, string password);

		[OperationContract]
		ActionMetaData PayEarly(int customerId, decimal amount, string loanRefNum);

		[OperationContract]
		ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId);

		[OperationContract]
		ActionMetaData PayPointNameValidationFailed(int userId, int customerId, string cardHolderName);

		[OperationContract]
		ActionMetaData RejectUser(int userId, int customerId);

		[OperationContract]
		ActionMetaData EmailRolloverAdded(int customerId, decimal amount);

		[OperationContract]
		ActionMetaData RenewEbayToken(int customerId, string marketplaceName, string eBayAddress);

		[OperationContract]
		ActionMetaData RequestCashWithoutTakenLoan(int customerId);

		[OperationContract]
		ActionMetaData SendEmailVerification(int customerId, string email, string address);

		[OperationContract]
		ActionMetaData ThreeInvalidAttempts(int customerId, string password);

		[OperationContract]
		ActionMetaData TransferCashFailed(int customerId);

		[OperationContract]
		ActionMetaData BrokerLeadSendInvitation(int nLeadID, string sBrokerContactEmail);

		#endregion email strategies

		#region Experian

		[OperationContract]
		ActionMetaData CheckExperianCompany(int customerId, bool forceCheck);

		[OperationContract]
		ActionMetaData CheckExperianConsumer(int customerId, int directorId);

		#endregion Experian

		#region Land Registry

		[OperationContract]
		string LandRegistryEnquiry(int customerId, string buildingNumber, string streetName, string cityName, string postCode);
		
		[OperationContract]
		string LandRegistryRes(int customerId, string titleNumber);
		
		#endregion Land Registry

		#region Main

		[OperationContract]
		ActionMetaData MainStrategy1(int uderwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison);

		[OperationContract]
		ActionMetaData MainStrategy2(int uderwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison, bool isUnderwriterForced);

		[OperationContract]
		ActionMetaData MainStrategySync1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison);

		#endregion Main

		#region mobile phone code

		[OperationContract]
		BoolActionResult GenerateMobileCode(string phone);

		[OperationContract]
		BoolActionResult ValidateMobileCode(string phone, string code);

		#endregion mobile phone code

		#region other strategies

		[OperationContract]
		ActionMetaData FirstOfMonthStatusNotifier();

		[OperationContract]
		ActionMetaData FraudChecker(int customerId, FraudMode mode);

		[OperationContract]
		ActionMetaData LateBy14Days();

		[OperationContract]
		ActionMetaData PayPointCharger();

		[OperationContract]
		ActionMetaData SetLateLoanStatus();

		[OperationContract]
		ActionMetaData UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep);
		
		[OperationContract]
		ActionMetaData UpdateTransactionStatus();

		[OperationContract]
		ActionMetaData XDaysDue();

		[OperationContract]
		ActionMetaData UpdateCurrencyRates();

		[OperationContract]
		CrmLookupsActionResult CrmLoadLookups();

		[OperationContract]
		SerializedDataTableActionResult GetSpResultTable(string spName, params string[] parameters);

		[OperationContract]
		BoolActionResult SaveBasicInterestRate(List<BasicInterestRate> basicInterestRates);

		[OperationContract]
		BoolActionResult SaveLoanOfferMultiplier(List<LoanOfferMultiplier> loanOfferMultipliers);

		[OperationContract]
		ActionMetaData PostcodeSaveLog(
			string sRequestType,
			string sUrl,
			string sStatus,
			string sResponseData,
			string sErrorMessage,
			int nUserID
		);

		#endregion other strategies

		#region Quick offer

		[OperationContract]
		QuickOfferActionResult QuickOffer(int customerId, bool saveOfferToDB);

		[OperationContract]
		QuickOfferActionResult QuickOfferWithPrerequisites(int customerId, bool saveOfferToDB);
		
		#endregion Quick offer

		#region Wizard

		[OperationContract]
		WizardConfigsActionResult GetWizardConfigs();

		[OperationContract]
		ActionMetaData FinishWizard(int customerId, int underwriterId);

		[OperationContract]
		StringActionResult GetCustomerState(int customerId);

		[OperationContract]
		IntActionResult GetCustomerStatusRefreshInterval();

		#endregion Wizard

		[OperationContract]
		ActionMetaData CreateUnderwriter(string name, string password, string role);

		[OperationContract]
		DateTimeActionResult GetExperianConsumerCacheDate(List<int> ids);

		[OperationContract]
		DateTimeActionResult GetExperianCompanyCacheDate(int customerId);
	} // interface IEzService
} // namespace EzService


