namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddLoanInterestFreeze : NewLoanBaseStrategy
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

        public override void NL_Execute() {

            NL_AddLog(LogType.Info, "Strategy Start", this.loanInterestFreeze, null, null, null);
            try
            {
                long newLoanId = -1;

                if (oldLoanId != null)
                    newLoanId = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", oldLoanId));

                if (newLoanId > 0)
                {
                    this.loanInterestFreeze.LoanID = newLoanId;
                    nlLoanInterestFreezeID = DB.ExecuteScalar<long>("NL_LoanInterestFreezeSave",
                    CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanInterestFreeze>("Tbl", this.loanInterestFreeze));
                    NL_AddLog(LogType.Info, "Strategy End", this.loanInterestFreeze, nlLoanInterestFreezeID, null, null);
                }
                else
                {
                    NL_AddLog(LogType.DataExsistense ,"Strategy Faild",this.loanInterestFreeze,null ,null,null);
                }



                // ReSharper disable once CatchAllClause
            }
            catch (Exception ex)
            {
                Log.Alert("Failed to save NL_InterestFreeze, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanInterestFreeze.LoanID, ex);
                Error = string.Format("Failed to save NL_InterestFreeze, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanInterestFreeze.LoanID, ex.Message);
                NL_AddLog(LogType.Error, "Strategy Faild", this.loanInterestFreeze, null, ex.ToString(), ex.StackTrace);

            }
        }//Execute


        public long nlLoanInterestFreezeID { get; set; }
    }//class AddInterestFreeze
}//ns
