namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddLoanInterestFreeze : AStrategy
    {

        public string Error { get; set; }
        private int? oldLoanId { get; set; }

        public long LoanInterestFreezeID { get; set; }

        private readonly NL_LoanInterestFreeze loanInterestFreeze;

        public AddLoanInterestFreeze(int? OldLoanId, NL_LoanInterestFreeze loanInterestFreeze)
        {
            this.oldLoanId = OldLoanId;            
            this.loanInterestFreeze = loanInterestFreeze;
        }//constructor

        public override string Name { get { return "AddLoanInterestFreeze"; } }

        public override void Execute()
        {
            try
            {

                if (oldLoanId != null)
                    this.loanInterestFreeze.LoanID = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", oldLoanId));

                nlLoanInterestFreezeID = DB.ExecuteScalar<long>("NL_LoanInterestFreezeSave",
                CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanInterestFreeze>("Tbl", this.loanInterestFreeze));

                // ReSharper disable once CatchAllClause
            }
            catch (Exception ex)
            {
                Log.Alert("Failed to save NL_InterestFreeze, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanInterestFreeze.LoanID, ex);
                Error = string.Format("Failed to save NL_InterestFreeze, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanInterestFreeze.LoanID, ex.Message);
            }
        }//Execute


        public long nlLoanInterestFreezeID { get; set; }
    }//class AddInterestFreeze
}//ns
