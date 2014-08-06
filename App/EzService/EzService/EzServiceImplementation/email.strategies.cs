namespace EzService.EzServiceImplementation {
	using System.Collections.Generic;
	using EzBob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		#region async

		public ActionMetaData ApprovedUser(int userId, int customerId, decimal loanAmount) {
			return Execute(customerId, userId, typeof(ApprovedUser), customerId, loanAmount);
		} // ApprovedUser

		public ActionMetaData CashTransferred(int customerId, decimal amount, string loanRefNum) {
			return Execute(customerId, null, typeof(CashTransferred), customerId, amount, loanRefNum);
		} // CashTransferred

		public ActionMetaData EmailUnderReview(int customerId) {
			return Execute(customerId, null, typeof(EmailUnderReview), customerId);
		} // EmailUnderReview

		public ActionMetaData Escalated(int customerId) {
			return Execute(customerId, customerId, typeof(Escalated), customerId);
		} // Escalated

		public ActionMetaData GetCashFailed(int customerId) {
			return Execute(customerId, null, typeof(GetCashFailed), customerId);
		} // GetCashFailed

		public ActionMetaData LoanFullyPaid(int customerId, string loanRefNum) {
			return Execute(customerId, null, typeof(LoanFullyPaid), customerId, loanRefNum);
		} // LoanFullyPaid

		public ActionMetaData MoreAmlAndBwaInformation(int userId, int customerId) {
			return Execute(customerId, userId, typeof(MoreAmlAndBwaInformation), customerId);
		} // MoreAmlAndBwaInformation

		public ActionMetaData MoreAmlInformation(int userId, int customerId) {
			return Execute(customerId, userId, typeof(MoreAmlInformation), customerId);
		} // MoreAmlInformation

		public ActionMetaData MoreBwaInformation(int userId, int customerId) {
			return Execute(customerId, userId, typeof(MoreBwaInformation), customerId);
		} // MoreBwaInformation

		public ActionMetaData PasswordRestored(int customerId) {
			return Execute(customerId, null, typeof(PasswordRestored), customerId);
		} // PasswordRestored

		public ActionMetaData PayEarly(int customerId, decimal amount, string loanRefNum) {
			return Execute(customerId, customerId, typeof(PayEarly), customerId, amount, loanRefNum);
		} // PayEarly

		public ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId) {
			return Execute(customerId, underwriterId, typeof(PayPointAddedByUnderwriter), customerId, cardno, underwriterName, underwriterId);
		} // PayPointAddedByUnderwriter

		public ActionMetaData PayPointNameValidationFailed(int userId, int customerId, string cardHolderName) {
			return Execute(customerId, userId, typeof(PayPointNameValidationFailed), customerId, cardHolderName);
		} // PayPointNameValidationFailed

		public ActionMetaData RejectUser(int userId, int customerId) {
			return Execute(customerId, userId, typeof(RejectUser), customerId);
		} // RejectUser

		public ActionMetaData EmailRolloverAdded(int customerId, decimal amount) {
			return Execute(customerId, customerId, typeof(EmailRolloverAdded), customerId, amount);
		} // EmailRolloverAdded

		public ActionMetaData RenewEbayToken(int customerId, string marketplaceName, string eBayAddress) {
			return Execute(customerId, customerId, typeof(RenewEbayToken), customerId, marketplaceName, eBayAddress);
		} // RenewEbayToken

		public ActionMetaData RequestCashWithoutTakenLoan(int customerId) {
			return Execute(customerId, null, typeof(RequestCashWithoutTakenLoan), customerId);
		} // RequestCashWithoutTakenLoan

		public ActionMetaData SendEmailVerification(int customerId, string email, string address) {
			return Execute(customerId, customerId, typeof(SendEmailVerification), customerId, email, address);
		} // SendEmailVerification

		public ActionMetaData ThreeInvalidAttempts(int customerId) {
			return Execute(customerId, null, typeof(ThreeInvalidAttempts), customerId);
		} // ThreeInvalidAttempts

		public ActionMetaData TransferCashFailed(int customerId) {
			return Execute(customerId, null, typeof(TransferCashFailed), customerId);
		} // TransferCashFailed

		public ActionMetaData VipRequest(int customerId, string fullname, string email, string phone) {
			return Execute(customerId, null, typeof(VipRequest), customerId, fullname, email, phone);
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

		#endregion async

		#region sync

		public ActionMetaData BrokerLeadSendInvitation(int nLeadID, string sBrokerContactEmail) {
			return ExecuteSync<BrokerLeadSendInvitation>(null, null, nLeadID, sBrokerContactEmail);
		} // BrokerLeadSendInvitation

		#endregion sync
	} // class EzServiceImplementation
} // namespace EzService
