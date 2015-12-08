namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Linq;
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

			this.strategyArgs = new object[] { customerID, loanID, stateDate, userID, getCalculatorState };

			if (customerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, Result, this.Error, null);
				return;
			}

			if (loanID == 0) {
				this.Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, Result, this.Error, null);
				return;
			}

			this.StateDate = stateDate ?? DateTime.UtcNow;

			Result = new NL_Model(customerID);

			Result.Loan = new NL_Loans() { LoanID = loanID };
			Result.UserID = userID;
			GetCalculatorState = getCalculatorState;

			LoanDAL = new LoanDAL();

			this.strategyArgs = new object[] { Result.CustomerID, Result.Loan.LoanID, this.StateDate, Result.UserID, GetCalculatorState };

		} // constructor

		public override string Name { get { return "GetLoanDBState"; } }

		public NL_Model Result { get; private set; }
		private readonly DateTime StateDate;
		public string Error;
		public bool GetCalculatorState { get; private set; }

		private readonly object[] strategyArgs;

		//[SetterProperty]
		public ILoanDAL LoanDAL { get; set; }

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		public override void Execute() {

			if (!string.IsNullOrEmpty(this.Error)) {
				throw new NL_ExceptionInputDataInvalid(this.Error);
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, Result, this.Error, null);

			try {
				// loan
				Result.Loan = LoanDAL.GetLoan(Result.Loan.LoanID);

				// histories
				Result.Loan.Histories.Clear();
				Result.Loan.Histories.AddRange(LoanDAL.GetLoanHistories(Result.Loan.LoanID, this.StateDate).ToList());

				// schedules of each history
				foreach (NL_LoanHistory h in Result.Loan.Histories) {
					h.Schedule = DB.Fill<NL_LoanSchedules>("NL_LoanSchedulesGet", CommandSpecies.StoredProcedure,new QueryParameter("@LoanID", Result.Loan.LoanID));
				}

				// loan fees
				Result.Loan.Fees.Clear();
				Result.Loan.Fees.AddRange(DB.Fill<NL_LoanFees>("NL_LoansFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", Result.Loan.LoanID)).ToList());

				// filter cnacelled/deleted fees on GetLoanDBState strategy
				// filter in Calculator according to CalculationDate
				//fees.Where(f => f.DisabledTime == null || f.DeletedByUserID ==0).ForEach(f => Result.Loan.Fees.Add(f));;

				// interest freezes
				Result.Loan.FreezeInterestIntervals.Clear();
				Result.Loan.FreezeInterestIntervals.AddRange(DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", Result.Loan.LoanID)).ToList());

				// filter cancelled (deactivated) periods
				// TODO: take in consideration stateDate
				// freezes.Where(fr => fr.DeactivationDate != null).ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));
				//freezes.ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));

				// loan options
				Result.Loan.LoanOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", Result.Loan.LoanID));

				// TODO combine all payments + transactions to one SP kogda nibud'

				// payments
				Result.Loan.Payments.Clear();
				Result.Loan.Payments.AddRange(DB.Fill<NL_Payments>("NL_PaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", Result.Loan.LoanID))); //.OrderBy(p=>p.PaymentTime)); 

				var schp = DB.Fill<NL_LoanSchedulePayments>("NL_LoanSchedulePaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", Result.Loan.LoanID));
				var fps = DB.Fill<NL_LoanFeePayments>("NL_LoanFeePaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", Result.Loan.LoanID));

				foreach (NL_Payments p in Result.Loan.Payments) {
					p.SchedulePayments.Clear();
					p.SchedulePayments.AddRange(schp.Where(sp=>sp.PaymentID == p.PaymentID).ToList());
	
					p.FeePayments.Clear();
					p.FeePayments.AddRange(fps.Where(fp=>fp.PaymentID == p.PaymentID).ToList());
				}

				// valid accepted rollover
				Result.Loan.AcceptedRollovers.AddRange(DB.Fill<NL_LoanRollovers>("NL_AcceptedRollovers", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", Result.Loan.LoanID)));

				if (!GetCalculatorState)
					return;

				// get loan state updated by calculator
				try {
					ALoanCalculator calc = new LegacyLoanCalculator(Result);
					calc.GetState();
				    Result = calc.WorkingModel;
				} catch (NoInitialDataException noInitialDataException) {
					this.Error = noInitialDataException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, this.Error, null);
				} catch (InvalidInitialInterestRateException invalidInitialInterestRateException) {
					this.Error = invalidInitialInterestRateException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, this.Error, null);
				} catch (NoLoanHistoryException noLoanHistoryException) {
					this.Error = noLoanHistoryException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, this.Error, null);
				} catch (InvalidInitialAmountException invalidInitialAmountException) {
					this.Error = invalidInitialAmountException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, this.Error, null);
				} catch (OverflowException overflowException) {
					this.Error = overflowException.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, this.Error, null);

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					this.Error = ex.Message;
					NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Result, this.Error, null);
				}

				NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, Result, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Error = ex.Message;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, this.Error, ex.ToString(), ex.StackTrace);
				Log.Alert(ex, "Failed to load loan state.");
			} // try
		} // Execute

	} // class GetLoanDBState
} // namespace