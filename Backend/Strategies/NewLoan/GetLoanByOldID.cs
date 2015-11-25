namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using Ezbob.Backend.Strategies.NewLoan.Exceptions;
    using Ezbob.Database;

    public class GetLoanByOldID : AStrategy
    {
        public GetLoanByOldID(int oldID)
        {
            this.OldLoanId = oldID;
        } // constructor

        public override string Name { get { return "GetLoan"; } }
        public string Error { get; private set; }
        public long LoanID { get; private set; }
        private readonly long OldLoanId;

        public override void Execute()
        {
            if (this.OldLoanId == 0)
            {
                Error = NL_ExceptionRequiredDataNotFound.OldLoan;
                return;
                ;
            }

            NL_AddLog(LogType.Info, "Strategy Start", this.OldLoanId, null, null, null);
            try
            {
                /*  Loan = DB.FillFirst<NL_Loans>("NL_LoansGet",
                      CommandSpecies.StoredProcedure,
                      new QueryParameter("@LoanID", this.LoanId)
                  );*/


                LoanID = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", this.OldLoanId));


                NL_AddLog(LogType.Info, "Strategy End", this.OldLoanId, this.LoanID, null, null);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                NL_AddLog(LogType.Error, "Strategy Faild", this.OldLoanId, Error, ex.ToString(), ex.StackTrace);
            }
        } // Execute

        //public NL_Loans Loan { get; private set; }

    } // class GetLastOffer
} // ns
