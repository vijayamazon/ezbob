namespace Ezbob.Backend.Strategies.NewLoan {
    using System;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddPacnetFundTransfer : AStrategy {
        public AddPacnetFundTransfer(NL_PacnetTransactions pacnetTransaction, int loanID) {
            this.pacnetTransaction = pacnetTransaction;
            this.loanID = loanID;
        }//constructor

        public override string Name { get { return "AddPacnetFundTransfer"; } }

        public override void Execute() {
            
            int fundTransferID = DB.ExecuteScalar<int>("NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_FundTransfers>("Tbl",
                new NL_FundTransfers {
                    Amount = this.pacnetTransaction.Amount,
                    IsActive = true,
                    LoanID = this.loanID,
                    TransferTime = this.pacnetTransaction.TransactionTime ?? DateTime.UtcNow
                }));

            this.pacnetTransaction.FundTransferID = fundTransferID;
            DB.ExecuteNonQuery("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_PacnetTransactions>("Tbl", this.pacnetTransaction)); 
        }//Execute


        public int CashRequestID { get; set; }

        private readonly NL_PacnetTransactions pacnetTransaction;
        private readonly int loanID;
    }//class AddPacnetFundTransfer
}//ns
