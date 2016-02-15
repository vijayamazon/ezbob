namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.MailStrategies;
	using EZBob.DatabaseLib.Model.Database;

	partial class EzServiceImplementation {
		public ActionMetaData ApprovedUser(int userId, int customerId, decimal loanAmount, int nValidHours, bool isFirst) {
			return Execute<ApprovedUser>(customerId, userId, customerId, loanAmount, nValidHours, isFirst);
		} // ApprovedUser

		public ActionMetaData CashTransferred(int customerId, decimal amount, string loanRefNum, bool isFirst) {
			return Execute<CashTransferred>(customerId, null, customerId, amount, loanRefNum, isFirst);
		} // CashTransferred

		public ActionMetaData Escalated(int customerId, int userId) {
			return Execute<Escalated>(customerId, customerId, customerId);
		} // Escalated

		public ActionMetaData GetCashFailed(int customerId) {
			return Execute<GetCashFailed>(customerId, null, customerId);
		} // GetCashFailed

		public ActionMetaData LoanFullyPaid(int customerId, string loanRefNum) {
			return Execute<LoanFullyPaid>(customerId, null, customerId, loanRefNum);
		} // LoanFullyPaid

		public ActionMetaData MoreAmlAndBwaInformation(int userId, int customerId) {
			return Execute<MoreAmlAndBwaInformation>(customerId, userId, customerId);
		} // MoreAmlAndBwaInformation

		public ActionMetaData MoreAmlInformation(int userId, int customerId) {
			return Execute<MoreAmlInformation>(customerId, userId, customerId);
		} // MoreAmlInformation

		public ActionMetaData MoreBwaInformation(int userId, int customerId) {
			return Execute<MoreBwaInformation>(customerId, userId, customerId);
		} // MoreBwaInformation

		public ActionMetaData PasswordRestored(int customerId) {
			return Execute<PasswordRestored>(customerId, null, customerId);
		} // PasswordRestored

		public ActionMetaData PayEarly(int customerId, decimal amount, string loanRefNum) {
			return Execute<PayEarly>(customerId, customerId, customerId, amount, loanRefNum);
		} // PayEarly

		public ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId) {
			return Execute<PayPointAddedByUnderwriter>(customerId, underwriterId, customerId, cardno, underwriterName, underwriterId);
		} // PayPointAddedByUnderwriter

		public ActionMetaData PayPointNameValidationFailed(int userId, int customerId, string cardHolderName) {
			return Execute<PayPointNameValidationFailed>(customerId, userId, customerId, cardHolderName);
		} // PayPointNameValidationFailed

		public ActionMetaData RejectUser(int userId, int customerId, bool bSendToCustomer) {
			return Execute<RejectUser>(customerId, userId, customerId, bSendToCustomer);
		} // RejectUser

		public ActionMetaData EmailRolloverAdded(int userId, int customerId, decimal amount) {
			return Execute<EmailRolloverAdded>(customerId, userId, customerId, amount);
		} // EmailRolloverAdded

		public ActionMetaData RenewEbayToken(int userId, int customerId, string marketplaceName, string eBayAddress) {
			return Execute<RenewEbayToken>(customerId, userId, customerId, marketplaceName, eBayAddress);
		} // RenewEbayToken

		public ActionMetaData RequestCashWithoutTakenLoan(int customerId) {
			return Execute<RequestCashWithoutTakenLoan>(customerId, null, customerId);
		} // RequestCashWithoutTakenLoan

		public ActionMetaData TransferCashFailed(int customerId) {
			return Execute<TransferCashFailed>(customerId, null, customerId);
		} // TransferCashFailed

		public ActionMetaData VipRequest(int customerId, string fullname, string email, string phone) {
			return Execute<Ezbob.Backend.Strategies.MailStrategies.VipRequest>(customerId, null, customerId, fullname, email, phone);
		} // TransferCashFailed

		public ActionMetaData BrokerForceResetCustomerPassword(int nUserID, int nCustomerID) {
			return Execute<BrokerForceResetCustomerPassword>(nCustomerID, nUserID, nCustomerID);
		} // BrokerForceResetCustomerPassword

		public ActionMetaData NotifySalesOnNewCustomer(int nCustomerID) {
			return Execute<NotifySalesOnNewCustomer>(nCustomerID, null, nCustomerID);
		} // NotifySalesOnNewCustomer

		public ActionMetaData EmailHmrcParsingErrors(int nCustomerID, int nCustomerMarketplaceID, SortedDictionary<string, string> oErrorsToEmail) {
			return Execute<EmailHmrcParsingErrors>(nCustomerID, null, nCustomerID, nCustomerMarketplaceID, oErrorsToEmail);
		} // EmailHmrcParsingErrors

		public ActionMetaData BrokerLeadSendInvitation(int nLeadID, string sBrokerContactEmail, CustomerOriginEnum origin) {
			return ExecuteSync<BrokerLeadSendInvitation>(null, null, nLeadID, sBrokerContactEmail, origin);
		} // BrokerLeadSendInvitation

		public ActionMetaData CustomerBankAccountIsAlreadyAddedEmail(int customerID) {
			return Execute<CustomerBankAccountIsAlreadyAddedEmail>(customerID, customerID, customerID);
		} // CustomerBankAccountIsAlreadyAddedEmail
	} // class EzServiceImplementation
} // namespace EzService
