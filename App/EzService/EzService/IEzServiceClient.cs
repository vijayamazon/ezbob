﻿using System.ServiceModel;

namespace EzService {
	#region interface IEzServiceClient

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceClient {
		[OperationContract]
		StringListActionResult GetStrategiesList();

		[OperationContract]
		ActionMetaData GreetingMailStrategy(int nCustomerID, string sConfirmationEmail);

		[OperationContract]
		ActionMetaData CustomerMarketplaceAdded(int nCustomerID, int nMarketplaceID);
		
		[OperationContract]
		ActionMetaData ApprovedUser(int customerId, decimal loanAmount);

		[OperationContract]
		ActionMetaData CashTransferred(int customerId, int amount);

		[OperationContract]
		ActionMetaData EmailUnderReview(int customerId);

		[OperationContract]
		ActionMetaData ActivateEscalated(int customerId);

		[OperationContract]
		ActionMetaData ActivateGetCashFailed(int customerId);

		[OperationContract]
		ActionMetaData ActivateLoanFullyPaid(int customerId, string loanRefNum);

		[OperationContract]
		ActionMetaData MoreAmLandBwaInformation(int customerId);

		[OperationContract]
		ActionMetaData MoreAmlInformation(int customerId);

		[OperationContract]
		ActionMetaData MoreBwaInformation(int customerId);

		[OperationContract]
		ActionMetaData PasswordChanged(int customerId, string password);

		[OperationContract]
		ActionMetaData PasswordRestored(int customerId, string password);

		[OperationContract]
		ActionMetaData PayEarly(int customerId, int amount, string loanRefNum);

		[OperationContract]
		ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId);

		[OperationContract]
		ActionMetaData PayPointNameValidationFailed(int customerId, string cardHolderName);

		[OperationContract]
		ActionMetaData RejectUser(int customerId);

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
		ActionMetaData CaisUpdate(int caisId);

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
		ActionMetaData UpdateMarketplaces(int customerId, int marketplaceId);

		[OperationContract]
		ActionMetaData UpdateAllMarketplaces(int customerId);

		[OperationContract]
		ActionMetaData UpdateTransactionStatus();

		[OperationContract]
		ActionMetaData XDaysDue();
	} // interface IEzServiceClient

	#endregion interface IEzServiceClient
} // namespace EzService
