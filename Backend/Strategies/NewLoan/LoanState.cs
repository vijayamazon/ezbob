namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies.NewLoan.DAL;
    using Ezbob.Database;

    /// <summary>
    /// Load NL Loan from DB into NL_Model
    /// </summary>
    public class LoanState : AStrategy
    {

        public LoanState(NL_Model nlModel, long loanID, DateTime? stateDate)
        {

            Result = nlModel;
            this.loanID = loanID;

            StateDate = stateDate ?? DateTime.UtcNow;

            LoanDAL = new LoanDAL();
        } // constructor

        public override string Name { get { return "LoanState"; } }

        public NL_Model Result { get; private set; }
        private readonly long loanID;
        public DateTime StateDate { get; set; }
        public string Error;

        //[SetterProperty]
        public ILoanDAL LoanDAL { get; set; }

        public override void Execute()
        {
            NL_AddLog(LogType.Info, "Strategy Start", this.Result,null, null, null);
            try
            {
                // TODO replace with DAL calls

                // loan
                Result.Loan = new NL_Loans();
                Result.Loan = LoanDAL.GetLoan(this.loanID);

                // histories
                Result.Loan.Histories.Clear();
                Result.Loan.Histories = LoanDAL.GetLoanHistories(this.loanID, StateDate);

                // schedules
                foreach (NL_LoanHistory h in Result.Loan.Histories)
                {
                    h.Schedule = DB.Fill<NL_LoanSchedules>("NL_LoanSchedulesGet",
                           CommandSpecies.StoredProcedure,
                           new QueryParameter("@LoanID", this.loanID),
                           new QueryParameter("@Now", StateDate)
                    );
                }

                // loan fees
                Result.Loan.Fees.Clear();
                Result.Loan.Fees = DB.Fill<NL_LoanFees>("NL_LoansFeesGet",
                       CommandSpecies.StoredProcedure,
                       new QueryParameter("@LoanID", this.loanID));

                // filter cnacelled/deleted fees on LoanState strategy
                // filter in Calculator according to CalculationDate
                //fees.Where(f => f.DisabledTime == null || f.DeletedByUserID ==0).ForEach(f => Result.Loan.Fees.Add(f));;

                // interest freezes
                Result.Loan.FreezeInterestIntervals.Clear();
                List<NL_LoanInterestFreeze> freezes = DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet",
                       CommandSpecies.StoredProcedure,
                       new QueryParameter("@LoanID", this.loanID));

                // filter cancelled (deactivated) periods
                // TODO: take in consideration stateDate
                // freezes.Where(fr => fr.DeactivationDate != null).ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));
                freezes.ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));

                // loan options
                Result.Loan.LoanOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet",
                       CommandSpecies.StoredProcedure,
                       new QueryParameter("@LoanID", this.loanID)
                );

                // TODO combine all payments+ transactions to one SP

                // payments (logical loan transactions)
                Result.Loan.Payments.Clear();
                Result.Loan.Payments = DB.Fill<NL_Payments>("NL_PaymentsGet",
                       CommandSpecies.StoredProcedure,
                       new QueryParameter("@LoanID", this.loanID),
                       new QueryParameter("@Now", StateDate)
                );

                foreach (NL_Payments p in Result.Loan.Payments)
                {

                    p.SchedulePayments.Clear();
                    p.SchedulePayments = DB.Fill<NL_LoanSchedulePayments>("NL_LoanSchedulePaymentsGet",
                           CommandSpecies.StoredProcedure,
                           new QueryParameter("@LoanID", this.loanID)
                    );

                    // mark payment time for each schedule payment
                    p.SchedulePayments.ForEach(sp => sp.PaymentDate = p.PaymentTime);

                    p.FeePayments.Clear();
                    p.FeePayments = DB.Fill<NL_LoanFeePayments>("NL_LoanFeePaymentsGet",
                           CommandSpecies.StoredProcedure,
                           new QueryParameter("@LoanID", this.loanID)
                    );
                }

                // ReSharper disable once CatchAllClause
                NL_AddLog(LogType.Info, "Strategy End", null,this.Result, null, null);
            }
            catch (Exception ex)
            {
                NL_AddLog(LogType.Error, "Strategy Faild", this.Result,null, ex.ToString(), ex.StackTrace);
                Log.Alert(ex, "Failed to load loan state.");
            } // try
        } // Execute
	
    } // class LoanState
} // namespace