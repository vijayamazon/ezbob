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
        ListInvestorsResult LoadInvestors(int underwriterID);

		[OperationContract]
		TransactionsDataResult LoadTransactionsData(int underwriterID, int investorID, int bankAccountTypeID);
	    [OperationContract]
	    IntActionResult SaveInvestorContactList(int underwriterID, int investorID, IEnumerable<InvestorContactModel> investorContacts);

		[OperationContract]
		BoolActionResult AddManualTransaction(int underwriterID, int investorAccountID, decimal transactionAmount, DateTime transactionDate, int bankAccountTypeID, string transactionComment, string bankTransactionRef);
	    [OperationContract]
	    IntActionResult SaveInvestorBanksList(int underwriterID, int investorID, IEnumerable<InvestorBankAccountModel> investorBanks);

		[OperationContract]
		ActionMetaData LinkLoanToInvestor(int userID, int customerID, int loanID);

		[OperationContract]
		ActionMetaData LinkLoanRepaymentToInvestor(int userID, int customerID, int loanID, int loanTransactionID, decimal transactionAmount, DateTime transactionDate);

		[OperationContract]
		SchedulerDataResult LoadSchedulerData(int underwriterID, int investorID);
	    [OperationContract]
        BoolActionResult ManageInvestorDetails(int underwriterID, InvestorModel investorDetails);

		[OperationContract]
		BoolActionResult UpdateSchedulerData(int underwriterID, int investorID, decimal monthlyFundingCapital, int fundsTransferDate, string fundsTransferSchedule, string repaymentsTransferSchedule);
	} // interface IEzServiceInvestor
} // namespace  
