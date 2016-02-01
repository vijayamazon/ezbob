namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.Strategies.Investor;
	using EzService.ActionResults.Investor;

	partial class EzServiceImplementation : IEzServiceInvestor {
		
     
       public ListInvestorsResult LoadInvestors(int underwriterID)
       {
           LoadInvestors strategy;
           var metadata = ExecuteSync(out strategy, null, underwriterID);
           return new ListInvestorsResult
           {
               MetaData = metadata,
               Investors = strategy.Result
           };

       }
	    public InvestorActionResult LoadInvestor(int underwriterID, int investorID) {
			LoadInvestor strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorID);
			return new InvestorActionResult {
				MetaData = metadata,
				Investor = strategy.Result
			};
		}
        public IntActionResult SaveInvestorBanksList(int underwriterID, int investorID, IEnumerable<InvestorBankAccountModel> investorBanks)
        {
            SaveInvestorBanksList strategy;
            var metadata = ExecuteSync(out strategy, null, underwriterID, investorID, investorBanks);
            return new IntActionResult
            {
                MetaData = metadata,
				Value = investorID
            };
        }
        public IntActionResult SaveInvestorContactList(int underwriterID, int investorID, IEnumerable<InvestorContactModel> investorContacts)
        {
            SaveInvestorContactList strategy;
            var metadata = ExecuteSync(out strategy, null, underwriterID, investorID, investorContacts);
            return new IntActionResult
            {
                MetaData = metadata,
				Value = investorID
            };
        }
		public IntActionResult CreateInvestor(int underwriterID, InvestorModel investor, IEnumerable<InvestorContactModel> investorContacts, IEnumerable<InvestorBankAccountModel> investorBanks) {
			CreateInvestor strategy;
            var metadata = ExecuteSync(out strategy, null, underwriterID, underwriterID, investor, investorContacts, investorBanks);
			return new IntActionResult{
				MetaData = metadata, 
				Value = strategy.InvestorID
			};
		}

		public BoolActionResult ManageInvestorContact(int underwriterID, InvestorContactModel investorContact) {
			ManageInvestorContact strategy;
            var metadata = ExecuteSync(out strategy, null, underwriterID, investorContact);
			return new BoolActionResult {
				MetaData = metadata,
				Value = strategy.Result
			};
		}
        public BoolActionResult ManageInvestorDetails(int underwriterID, InvestorModel investorDetails)
        {
            ManageInvestorDetails strategy;
            var metadata = ExecuteSync(out strategy, null, underwriterID, investorDetails);
            return new BoolActionResult
            {
                MetaData = metadata,
                Value = strategy.Result
            };
        }
		public BoolActionResult ManageInvestorBankAccount(int underwriterID, InvestorBankAccountModel investorBank) {
			ManageInvestorBankAccount strategy;
            var metadata = ExecuteSync(out strategy, null, underwriterID, underwriterID, investorBank);
			return new BoolActionResult {
				MetaData = metadata,
				Value = strategy.Result
			};
		}


		public AccountingDataResult LoadAccountingData(int underwriterID) {
			LoadAccountingData strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID);
			return new AccountingDataResult {
				MetaData = metadata,
				AccountingData = strategy.Result
			};
		}

		public TransactionsDataResult LoadTransactionsData(int underwriterID, int investorID, int bankAccountTypeID) {
			LoadTransactionsData strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorID, bankAccountTypeID);
			return new TransactionsDataResult {
				MetaData = metadata,
				TransactionsData = strategy.Result
			};
		}

		public SchedulerDataResult LoadSchedulerData(int underwriterID, int investorID) {
			LoadSchedulerData strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorID);
			return new SchedulerDataResult {
				MetaData = metadata,
				SchedulerData = strategy.Result
			};
		}

		public BoolActionResult UpdateSchedulerData(int underwriterID, int investorID, decimal monthlyFundingCapital, int fundsTransferDate, string fundsTransferSchedule, string repaymentsTransferSchedule) {
			UpdateSchedulerData strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, underwriterID, investorID, monthlyFundingCapital, fundsTransferDate, fundsTransferSchedule, repaymentsTransferSchedule);
			return new BoolActionResult {
				MetaData = metadata,
				Value = strategy.Result
			};
		}

		public BoolActionResult AddManualTransaction(int underwriterID, int investorAccountID, decimal transactionAmount, DateTime transactionDate, int bankAccountTypeID, string transactionComment, string bankTransactionRef) {
			AddManualTransaction strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, underwriterID, investorAccountID, transactionAmount, transactionDate, bankAccountTypeID, transactionComment, bankTransactionRef);
			return new BoolActionResult {
				MetaData = metadata,
				Value = strategy.Result
			};
		}

		public ActionMetaData LinkLoanToInvestor(int userID, int customerID, int loanID) {
			var metadata = Execute<LinkLoanToInvestor>(customerID, userID, customerID, loanID);
			return metadata;
		}

		public ActionMetaData LinkLoanRepaymentToInvestor(int userID, int customerID, int loanID, int loanTransactionID, decimal transactionAmount, DateTime transactionDate) {
			Log.Info("EzServiceInvestor LinkLoanRepaymentToInvestor {0}", loanTransactionID);
			var metadata = Execute<LinkRepaymentToInvestor>(customerID, userID, 
				loanID, loanTransactionID, transactionAmount, transactionDate, userID);
			return metadata;
		}
	}
}

