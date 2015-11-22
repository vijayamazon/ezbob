namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class GetLoan : AStrategy
    {
        public GetLoan(long loanId)
        {
            this.LoanId = loanId;
        } // constructor

        public override string Name { get { return "GetLoan"; } }

        public override void Execute()
        {
            NL_AddLog(LogType.Info, "Strategy Start", this.LoanId, null, null, null);
            try
            {
                Loan = DB.FillFirst<NL_Loans>("NL_LoansGet",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("CustomerID", this.LoanId)
                );
                NL_AddLog(LogType.Info, "Strategy End", this.LoanId, this.Loan, null, null);
            }
            catch (Exception ex)
            {
                NL_AddLog(LogType.Error, "Strategy Faild", this.LoanId, null, ex.ToString(), ex.StackTrace);
            }
        } // Execute

        public NL_Loans Loan { get; private set; }
        private readonly long LoanId;
    } // class GetLastOffer
} // ns
