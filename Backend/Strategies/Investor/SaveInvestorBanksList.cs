using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.Backend.Strategies.Investor
{
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Database;

    public class SaveInvestorBanksList : AStrategy
    {
        public SaveInvestorBanksList(int investor, IEnumerable<InvestorBankAccountModel> banks)
        {

            this.InvestorID = investor;
			this.Banks = banks;
		
		}//ctor
          public override string Name { get { return "SaveInvestorBanksList"; } }
         public override void Execute()
        {
            DateTime now = DateTime.UtcNow;
            try {
                var dbBanks = new List<I_InvestorBankAccount>();
                foreach (var bank in this.Banks)
                {
                    dbBanks.Add(new I_InvestorBankAccount
                    {
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
                        InvestorID = InvestorID
                    });
                }


                DB.ExecuteNonQuery("I_InvestorBankAccountUpdate", CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<I_InvestorBankAccount>("Tbl", dbBanks)
                    );
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to edit investor Bank on DB");
              
                Result = false;
                throw;
            }
            Result = true;

        }
          public bool Result { get; set; }
        public int InvestorID { get; private set; }

        private readonly IEnumerable<InvestorBankAccountModel> Banks;
    }
}



