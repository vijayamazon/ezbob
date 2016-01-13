namespace EzService {
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using Ezbob.Backend.Models.Investor;
	using EzService.ActionResults.Investor;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceInvestor {
		[OperationContract]
		IntActionResult CreateInvestor(int underwriterID, InvestorModel investor, IEnumerable<InvestorContactModel> investorContacts, IEnumerable<InvestorBankAccountModel> investorBanks);

		[OperationContract]
		InvestorActionResult LoadInvestor(int underwriterID, int investorID);

		[OperationContract]
		BoolActionResult ManageInvestorContact(int underwriterID, InvestorContactModel investorContact);

		[OperationContract]
		BoolActionResult ManageInvestorBankAccount(int underwriterID, InvestorBankAccountModel investorBank);

		[OperationContract]
		AccountingDataResult LoadAccountingData(int underwriterID);

		[OperationContract]
		TransactionsDataResult LoadTransactionsData(int underwriterID, int investorID, int bankAccountTypeID);

		[OperationContract]
		BoolActionResult AddManualTransaction(int underwriterID, int investorAccountID, decimal transactionAmount, DateTime transactionDate, int bankAccountTypeID, string transactionComment);

		[OperationContract]
		ActionMetaData LinkLoanToInvestor(int userID, int customerID, int loanID);

		[OperationContract]
		ActionMetaData LinkLoanRepaymentToInvestor(int userID, int customerID, int loanID, int loanTransactionID, decimal transactionAmount, DateTime transactionDate);
	} // interface IEzServiceInvestor
} // namespace  
