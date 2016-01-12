namespace EzService.EzServiceImplementation {
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.Investor;
	using Ezbob.Backend.Strategies.Investor;
	using EzService.ActionResults.Investor;

	partial class EzServiceImplementation : IEzServiceInvestor {
		
		public InvestorActionResult LoadInvestor(int underwriterID, int investorID) {
			LoadInvestor strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorID);
			return new InvestorActionResult {
				MetaData = metadata,
				Investor = strategy.Result
			};
		}

		public IntActionResult CreateInvestor(int underwriterID, InvestorModel investor, IEnumerable<InvestorContactModel> investorContacts, IEnumerable<InvestorBankAccountModel> investorBanks) {
			CreateInvestor strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investor, investorContacts, investorBanks);
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

		public BoolActionResult ManageInvestorBankAccount(int underwriterID, InvestorBankAccountModel investorBank) {
			ManageInvestorBankAccount strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorBank);
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
	}
}

