namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class SaveInvestorBanksList : AStrategy {
		public SaveInvestorBanksList(int investorID, IEnumerable<InvestorBankAccountModel> banks) {
			this.investorID = investorID;
			this.banks = banks;
		} //ctor

		public override string Name { get { return "SaveInvestorBanksList"; } }
		public override void Execute() {
			DateTime now = DateTime.UtcNow;
			try {
				var dbBanks = new List<I_InvestorBankAccount>();
				foreach (var bank in this.banks) {
					dbBanks.Add(new I_InvestorBankAccount {
						IsActive = bank.IsActive,
						Timestamp = now,
						BankAccountNumber = bank.BankAccountNumber,
						BankAccountName = bank.BankAccountName,
						BankBranchName = bank.BankBranchName,
						BankBranchNumber = bank.BankBranchNumber,
						BankCode = bank.BankCode,
						BankCountryID = bank.BankCountryID,
						BankName = bank.BankName,
						InvestorAccountTypeID = bank.AccountType.InvestorAccountTypeID,
						InvestorBankAccountID = bank.InvestorBankAccountID,
						RepaymentKey = bank.RepaymentKey,
						InvestorID = this.investorID
					});
				}


				DB.ExecuteNonQuery("I_InvestorBankAccountUpdateActive", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<I_InvestorBankAccount>("Tbl", dbBanks)
					);
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to edit investor Bank on DB");

				Result = false;
				throw;
			}
			Result = true;
		}//Execute

		public bool Result { get; set; }
		private readonly int investorID;
		private readonly IEnumerable<InvestorBankAccountModel> banks;
	}//SaveInvestorBanksList
}//ns



