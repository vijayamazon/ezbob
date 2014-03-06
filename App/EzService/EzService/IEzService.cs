﻿using System.ServiceModel;

namespace EzService {
	using EzBob.Backend.Strategies;
	using FraudChecker;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzService {
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
		ActionMetaData CaisGenerate(int underwriterId);

		[OperationContract]
		ActionMetaData CaisUpdate(int userId, int caisId);

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
		ActionMetaData CheckExperianCompany(int customerId);

		[OperationContract]
		ActionMetaData CheckExperianConsumer(int customerId, int directorId);

		[OperationContract]
		ActionMetaData CheckAml(int customerId);

		[OperationContract]
		ActionMetaData CheckAmlCustom(int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet, string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode);

		[OperationContract]
		ActionMetaData CheckBwa(int customerId);

		[OperationContract]
		ActionMetaData CheckBwaCustom(int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet, string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode, string idhubBranchCode, string idhubAccountNumber);

		[OperationContract]
		ActionMetaData MainStrategy1(int uderwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison);

		[OperationContract]
		ActionMetaData MainStrategy2(int uderwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison, bool isUnderwriterForced);

		[OperationContract]
		ActionMetaData MainStrategy3(int uderwriterId, int customerId, int checkType, string houseNumber, string houseName, string street, string district, string town, string county, string postcode, string bankAccount, string sortCode, int avoidAutoDescison);

		[OperationContract]
		ActionMetaData MainStrategySync1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison);
		
		[OperationContract]
		BoolActionResult GenerateMobileCode(string phone);

		[OperationContract]
		BoolActionResult ValidateMobileCode(string phone, string code);

		[OperationContract]
		WizardConfigsActionResult GetWizardConfigs();

		[OperationContract]
		ActionMetaData UpdateCurrencyRates();

		#region Quick offer

		[OperationContract]
		QuickOfferActionResult QuickOffer(int customerId, bool saveOfferToDB);

		[OperationContract]
		QuickOfferActionResult QuickOfferWithPrerequisites(int customerId, bool saveOfferToDB);
		
		#endregion Quick offer

		[OperationContract]
		ActionMetaData FinishWizard(int customerId);

		[OperationContract]
		CrmLookupsActionResult CrmLoadLookups();

		#region Broker

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
			string Password2
		);

		[OperationContract]
		ActionMetaData BrokerLogin(string Email, string Password);

		[OperationContract]
		ActionMetaData BrokerRestorePassword(string sMobile, string sCode);

		[OperationContract]
		BrokerCustomersActionResult BrokerLoadCustomerList(string sContactEmail);

		[OperationContract]
		BrokerCustomerDetailsActionResult BrokerLoadCustomerDetails(int nCustomerID, string sContactEmail);

		[OperationContract]
		StringActionResult BrokerSaveCrmEntry(bool bIsIncoming, int nActionID, int nStatusID, string sComment, int nCustomerID, string sContactEmail);

		[OperationContract]
		BrokerCustomerFilesActionResult BrokerLoadCustomerFiles(int nCustomerID, string sContactEmail);

		[OperationContract]
		BrokerCustomerFileContentsActionResult BrokerDownloadCustomerFile(int nCustomerID, string sContactEmail, int nFileID);

		[OperationContract]
		ActionMetaData BrokerSaveUploadedCustomerFile(int nCustomerID, string sContactEmail, byte[] oFileContents, string sFileName);

		#endregion Broker

		#region Land Registry

		[OperationContract]
		string LandRegistryEnquiry(int customerId, string buildingNumber, string streetName, string cityName, string postCode);
		
		[OperationContract]
		string LandRegistryRes(int customerId, string titleNumber);
		
		#endregion Land Registry

		#region Company Files

		[OperationContract]
		ActionMetaData CompanyFilesUpload(int customerId, string fileName, byte[] fileContext);

		[OperationContract]
		byte[] GetCompanyFile(int companyFileId);

		#endregion

	} // interface IEzService
} // namespace EzService


