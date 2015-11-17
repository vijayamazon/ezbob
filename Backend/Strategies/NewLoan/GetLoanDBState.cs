namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies.NewLoan.DAL;
    using Ezbob.Database;

    /// <summary>
    /// Load NL Loan from DB into NL_Model. nlModel argument should be empty - filled in from within the strategy
    /// </summary>
    public class GetLoanDBState : AStrategy{

        public GetLoanDBState(NL_Model nlModel, long loanID, DateTime? stateDate){

            Result = nlModel;
            this.loanID = loanID;

            StateDate = stateDate ?? DateTime.UtcNow;

            LoanDAL = new LoanDAL();

			this.strategyArgs = new object[] { Result, this.loanID, StateDate };
        } // constructor

        public override string Name { get { return "GetLoanDBState"; } }

        public NL_Model Result { get; private set; }
        private readonly long loanID;
        public DateTime StateDate { get; set; }
        public string Error;

		private readonly object[] strategyArgs;

        //[SetterProperty]
        public ILoanDAL LoanDAL { get; set; }

        public override void Execute(){

            NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, Result, null, null);

            try
            {
                // loan
                Result.Loan = new NL_Loans();
                Result.Loan = LoanDAL.GetLoan(this.loanID);

                // histories
                Result.Loan.Histories.Clear();
                Result.Loan.Histories = LoanDAL.GetLoanHistories(this.loanID, StateDate);

                // schedules
                foreach (NL_LoanHistory h in Result.Loan.Histories){
                    h.Schedule = DB.Fill<NL_LoanSchedules>("NL_LoanSchedulesGet",CommandSpecies.StoredProcedure,
                           new QueryParameter("@LoanID", this.loanID),
                           new QueryParameter("@Now", StateDate)
                    );
                }

                // loan fees
                Result.Loan.Fees.Clear();
                Result.Loan.Fees = DB.Fill<NL_LoanFees>("NL_LoansFeesGet",CommandSpecies.StoredProcedure,
                       new QueryParameter("@LoanID", this.loanID));

                // filter cnacelled/deleted fees on GetLoanDBState strategy
                // filter in Calculator according to CalculationDate
                //fees.Where(f => f.DisabledTime == null || f.DeletedByUserID ==0).ForEach(f => Result.Loan.Fees.Add(f));;

                // interest freezes
                Result.Loan.FreezeInterestIntervals.Clear();
                List<NL_LoanInterestFreeze> freezes = DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet",CommandSpecies.StoredProcedure,
                       new QueryParameter("@LoanID", this.loanID));

                // filter cancelled (deactivated) periods
                // TODO: take in consideration stateDate
                // freezes.Where(fr => fr.DeactivationDate != null).ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));
                freezes.ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));

                // loan options
                Result.Loan.LoanOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet",CommandSpecies.StoredProcedure,
                       new QueryParameter("@LoanID", this.loanID)
                );

                // TODO combine all payments + transactions to one SP kogda nibud'

                // payments (logical transactions) ordered by PaymentTime
                Result.Loan.Payments.Clear();
                Result.Loan.Payments = new List<NL_Payments>(DB.Fill<NL_Payments>("NL_PaymentsGet",CommandSpecies.StoredProcedure,
	                new QueryParameter("@LoanID", this.loanID),
	                new QueryParameter("@Now", StateDate)
	                ).OrderBy(p=>p.PaymentTime));

                foreach (NL_Payments p in Result.Loan.Payments){
                    p.SchedulePayments.Clear();
                    p.SchedulePayments = DB.Fill<NL_LoanSchedulePayments>("NL_LoanSchedulePaymentsGet",CommandSpecies.StoredProcedure,
                           new QueryParameter("@LoanID", this.loanID)
                    );

                    // mark payment time for each schedule payment
                    p.SchedulePayments.ForEach(sp => sp.PaymentDate = p.PaymentTime);

                    p.FeePayments.Clear();
                    p.FeePayments = DB.Fill<NL_LoanFeePayments>("NL_LoanFeePaymentsGet",CommandSpecies.StoredProcedure,
                           new QueryParameter("@LoanID", this.loanID)
                    );
                }

				// valid rollover (StateDate between rollover expiration and creation time
				Result.Loan.Rollover = DB.FillFirst<NL_LoanRollovers>("NL_ValidRollover",CommandSpecies.StoredProcedure,
					   new QueryParameter("@LoanID", this.loanID), 
					   new QueryParameter("@Now", StateDate)
				);

                // ReSharper disable once CatchAllClause
                NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, Result, null, null);

            } catch (Exception ex) {
	            this.Error = ex.Message;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, this.Error, ex.ToString(), ex.StackTrace);
                Log.Alert(ex, "Failed to load loan state.");
            } // try
        } // Execute
	
    } // class GetLoanDBState
} // namespace