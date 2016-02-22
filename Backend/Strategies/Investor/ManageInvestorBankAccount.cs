namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class ManageInvestorBankAccount : AStrategy {
		public ManageInvestorBankAccount(int underwriterID,InvestorBankAccountModel bank) {
			this.bank = bank;
            this.underwriterID = underwriterID;
		}//ctor

		public override string Name { get { return "ManageInvestorBankAccount"; } }

		public override void Execute() {
			DateTime now = DateTime.UtcNow;
			var con = DB.GetPersistent();
			con.BeginTransaction();

			try {
				var dbBank = new I_InvestorBankAccount {
					IsActive = this.bank.IsActive,
					Timestamp = now,
					BankAccountNumber = this.bank.BankAccountNumber,
					BankAccountName = this.bank.BankAccountName,
					BankBranchName = this.bank.BankBranchName,
					BankBranchNumber = this.bank.BankBranchNumber,
					BankCode = this.bank.BankCode,
					BankCountryID = this.bank.BankCountryID,
					BankName = this.bank.BankName,
					InvestorAccountTypeID = this.bank.AccountType.InvestorAccountTypeID,
					InvestorBankAccountID = this.bank.InvestorBankAccountID,
					RepaymentKey = this.bank.RepaymentKey,
					InvestorID = this.bank.InvestorID,
                    UserID=this.underwriterID        
				};

				DB.ExecuteNonQuery(con, this.bank.InvestorBankAccountID == 0 ? "I_InvestorBankAccountSave" : "I_InvestorBankAccountUpdate", 
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<I_InvestorBankAccount>("Tbl", new List<I_InvestorBankAccount> { dbBank })
				);
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to save investor {0} bank data to DB", this.bank.InvestorID);
				con.Rollback();
				Result = false;
				throw;
			}//try

			con.Commit();
			Result = true;
			Log.Info("Manage investor {0} bank data into DB complete.", this.bank.InvestorID);
		}//Execute

        private readonly int underwriterID;
		public bool Result { get; set; }
		private readonly InvestorBankAccountModel bank;
	}//ManageInvestorBankAccount
}//ns
