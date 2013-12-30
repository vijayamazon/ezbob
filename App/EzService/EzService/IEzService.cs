using System.ServiceModel;

namespace EzService {
	using EzBob.Backend.Strategies;

	#region interface IEzService

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
		ActionMetaData SendEmailVerification(int customerId, string address);

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
		ActionMetaData FraudChecker(int customerId);

		[OperationContract]
		ActionMetaData LateBy14Days();

		[OperationContract]
		ActionMetaData PayPointCharger();

		[OperationContract]
		ActionMetaData SetLateLoanStatus();

		[OperationContract]
		ActionMetaData UpdateMarketplace(int customerId, int marketplaceId);

		[OperationContract]
		ActionMetaData UpdateAllMarketplaces(int customerId);

		[OperationContract]
		ActionMetaData UpdateTransactionStatus();

		[OperationContract]
		ActionMetaData XDaysDue();

		[OperationContract]
		ActionMetaData MainStrategy1(int uderwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison);

		[OperationContract]
		ActionMetaData MainStrategy2(int uderwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison, bool isUnderwriterForced);

		[OperationContract]
		ActionMetaData MainStrategy3(int uderwriterId, int customerId, int checkType, string houseNumber, string houseName, string street, string district, string town, string county, string postcode, string bankAccount, string sortCode, int avoidAutoDescison);
		
		[OperationContract]
		ActionMetaData GenerateMobileCode(string phone);

		[OperationContract]
		BoolActionResult ValidateMobileCode(string phone, string code);

		[OperationContract]
		WizardConfigsActionResult GetWizardConfigs();
	} // interface IEzService

	#endregion interface IEzService
} // namespace EzService


