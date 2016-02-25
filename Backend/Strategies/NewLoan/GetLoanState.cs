namespace Ezbob.Backend.Strategies.NewLoan {
    using System;
    using System.Linq;
    using ConfigManager;
    using Ezbob.Backend.CalculateLoan.LoanCalculator;
    using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies.NewLoan.DAL;
    using Ezbob.Backend.Strategies.NewLoan.Exceptions;
    using Ezbob.Database;

    /// <summary>
	/// Load NL Loan from DB into NL_Model. Return state of the loan in nlModel with calculated data from the calculator
	/// </summary>
	public class GetLoanState : AStrategy {

		public GetLoanState(int customerID, long loanID, DateTime? stateDate, int? userID = null, bool getCalculatorState = true) {

			//this.strategyArgs = new object[] { customerID, loanID, stateDate, userID, getCalculatorState };

			this.StateDate = stateDate ?? DateTime.UtcNow;

			Result = new NL_Model(customerID);

			Result.Loan = new NL_Loans() { LoanID = loanID };
			Result.UserID = userID;
			GetCalculatorState = getCalculatorState;

			LoanDAL = new LoanDAL();

			this.strategyArgs = new object[] { Result.CustomerID, Result.Loan.LoanID, this.StateDate, Result.UserID, GetCalculatorState };

		} // constructor

        public override string Name { get { return "GetLoanState"; } }

		public NL_Model Result { get; private set; }
		private readonly DateTime StateDate;
		public string Error { get; private set; }
		public bool GetCalculatorState { get; private set; }

		private readonly object[] strategyArgs;

		//[SetterProperty]
		public ILoanDAL LoanDAL { get; set; }

		
		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			if (Result.CustomerID == 0) {
				Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, Result, Error, null);
				return;
			}

			if (Result.Loan.LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, Result, Error, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, Result, Error, null);

			try {

				// loan
				Result.Loan = LoanDAL.GetLoan(Result.Loan.LoanID);
				
				// offer-fees
				//Result.Offer.OfferFees = DB.Fill<NL_OfferFees>("NL_OfferFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("OfferID", Result.Loan.OfferID));

				// histories
				Result.Loan.Histories.Clear();
				Result.Loan.Histories.AddRange(LoanDAL.GetLoanHistories(Result.Loan.LoanID, this.StateDate).ToList());

				// schedules of each history
				foreach (NL_LoanHistory h in Result.Loan.Histories) {
					h.Schedule = DB.Fill<NL_LoanSchedules>("NL_LoanSchedulesGet", CommandSpecies.StoredProcedure,new QueryParameter("LoanID", Result.Loan.LoanID));
				}

				// loan fees
				Result.Loan.Fees.Clear();
				Result.Loan.Fees.AddRange(DB.Fill<NL_LoanFees>("NL_LoanFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", Result.Loan.LoanID)).ToList());

				// filter cnacelled/deleted fees on GetLoanDBState strategy
				// filter in Calculator according to CalculationDate
				//fees.Where(f => f.DisabledTime == null || f.DeletedByUserID ==0).ForEach(f => Result.Loan.Fees.Add(f));;

				// interest freezes
				Result.Loan.FreezeInterestIntervals.Clear();
				Result.Loan.FreezeInterestIntervals.AddRange(DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", Result.Loan.LoanID)).ToList());

				// filter cancelled (deactivated) periods
				// TODO: take in consideration stateDate
				// freezes.Where(fr => fr.DeactivationDate != null).ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));
				//freezes.ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));

				// loan options
				Result.Loan.LoanOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", Result.Loan.LoanID));

				// TODO combine all payments + transactions to one SP kogda nibud'

				// payments
				Result.Loan.Payments.Clear();
				Result.Loan.Payments.AddRange(DB.Fill<NL_Payments>("NL_PaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", Result.Loan.LoanID)));

				var ppt = DB.Fill<NL_PaypointTransactions>("NL_PaypointTransactionsGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", Result.Loan.LoanID));
				var schp = DB.Fill<NL_LoanSchedulePayments>("NL_LoanSchedulePaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", Result.Loan.LoanID));
				var fps = DB.Fill<NL_LoanFeePayments>("NL_LoanFeePaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", Result.Loan.LoanID));

				foreach (NL_Payments p in Result.Loan.Payments) {
					p.SchedulePayments.Clear();
					p.SchedulePayments.AddRange(schp.Where(sp=>sp.PaymentID == p.PaymentID).ToList());
	
					p.FeePayments.Clear();
					p.FeePayments.AddRange(fps.Where(fp=>fp.PaymentID == p.PaymentID).ToList());

					p.PaypointTransactions.Clear();
					p.PaypointTransactions.AddRange(ppt.Where(pp => pp.PaymentID == p.PaymentID).ToList());
				}

				// set paid amount for each fee
				foreach (NL_LoanFees fee in Result.Loan.Fees) {
					fee.PaidAmount = fps.Where(fp => fp.LoanFeeID == fee.LoanFeeID).Sum(fp => fp.Amount);
				}

				// set paid amount for each fee
				foreach (NL_LoanHistory h in Result.Loan.Histories) {
					foreach (NL_LoanSchedules s in h.Schedule) {
						s.InterestPaid = schp.Where(sp => sp.LoanScheduleID == s.LoanScheduleID).Sum(sp => sp.InterestPaid);
						s.PrincipalPaid = schp.Where(sp => sp.LoanScheduleID == s.LoanScheduleID).Sum(sp => sp.PrincipalPaid);
					}
				}

				Result.Loan.Rollovers.Clear();
				Result.Loan.Rollovers.AddRange(DB.Fill<NL_LoanRollovers>("NL_RolloversGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", Result.Loan.LoanID)));

				// accepted rollover
				Result.Loan.AcceptedRollovers.AddRange(Result.Loan.Rollovers.Where(r => r.IsAccepted && r.CustomerActionTime.HasValue &&r.DeletionTime==null && r.DeletedByUserID==null));

				if (!GetCalculatorState)
					return;

				// get loan state updated by calculator
				try {
					ALoanCalculator calc = new LegacyLoanCalculator(Result);
					calc.GetState();
					Result = calc.WorkingModel;
				} catch (NoInitialDataException noInitialDataException) {
					Error = noInitialDataException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);
				} catch (InvalidInitialInterestRateException invalidInitialInterestRateException) {
					Error = invalidInitialInterestRateException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);
				} catch (NoLoanHistoryException noLoanHistoryException) {
					Error = noLoanHistoryException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);
				} catch (InvalidInitialAmountException invalidInitialAmountException) {
					Error = invalidInitialAmountException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);
				} catch (OverflowException overflowException) {
					Error = overflowException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);
				} catch (LoanPaidOffStatusException loanPaidOffexException) {
					Error = loanPaidOffexException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);
				} catch (LoanWriteOffStatusException loanWriteoffexException) {
					Error = loanWriteoffexException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);
				} catch (LoanPendingStatusException loanPendingException) {
					Error = loanPendingException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Error = ex.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, Error, null);
				}

				NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, Result, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				NL_AddLog(LogType.Error, "Strategy Failed", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
				Log.Alert(ex, "Failed to load loan state.");
			} // try
		} // Execute

	} // class GetLoanDBState
} // namespace