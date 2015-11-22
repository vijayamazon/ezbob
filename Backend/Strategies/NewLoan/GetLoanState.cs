namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
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

		public GetLoanState(int customerID, long loanID, DateTime? stateDate, int? userID = null) {

			this.strategyArgs = new object[] { customerID, loanID, stateDate, userID };

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

			LoanDAL = new LoanDAL();

			this.strategyArgs = new object[] { Result.CustomerID, Result.Loan.LoanID, this.StateDate, Result.UserID };

		} // constructor

		public override string Name { get { return "GetLoanDBState"; } }

		public NL_Model Result { get; private set; }
		private readonly DateTime StateDate;
		public string Error;

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
				Result.Loan.Histories = LoanDAL.GetLoanHistories(Result.Loan.LoanID, this.StateDate);

				// schedules
				foreach (NL_LoanHistory h in Result.Loan.Histories) {
					h.Schedule = DB.Fill<NL_LoanSchedules>("NL_LoanSchedulesGet", CommandSpecies.StoredProcedure,
						   new QueryParameter("@LoanID", Result.Loan.LoanID),
						   new QueryParameter("@Now", this.StateDate)
					);
				}

				// loan fees
				Result.Loan.Fees.Clear();
				Result.Loan.Fees = DB.Fill<NL_LoanFees>("NL_LoansFeesGet", CommandSpecies.StoredProcedure,
					   new QueryParameter("@LoanID", Result.Loan.LoanID));

				// filter cnacelled/deleted fees on GetLoanDBState strategy
				// filter in Calculator according to CalculationDate
				//fees.Where(f => f.DisabledTime == null || f.DeletedByUserID ==0).ForEach(f => Result.Loan.Fees.Add(f));;

				// interest freezes
				Result.Loan.FreezeInterestIntervals.Clear();
				List<NL_LoanInterestFreeze> freezes = DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet", CommandSpecies.StoredProcedure,
					   new QueryParameter("@LoanID", Result.Loan.LoanID));

				// filter cancelled (deactivated) periods
				// TODO: take in consideration stateDate
				// freezes.Where(fr => fr.DeactivationDate != null).ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));
				freezes.ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));

				// loan options
				Result.Loan.LoanOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure,
					   new QueryParameter("@LoanID", Result.Loan.LoanID)
				);

				// TODO combine all payments + transactions to one SP kogda nibud'

				// payments (logical transactions) ordered by PaymentTime
				Result.Loan.Payments.Clear();
				Result.Loan.Payments = new List<NL_Payments>(DB.Fill<NL_Payments>("NL_PaymentsGet", CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", Result.Loan.LoanID),
					new QueryParameter("@Now", StateDate)
					).OrderBy(p => p.PaymentTime));

				foreach (NL_Payments p in Result.Loan.Payments) {
					p.SchedulePayments.Clear();
					p.SchedulePayments = DB.Fill<NL_LoanSchedulePayments>("NL_LoanSchedulePaymentsGet", CommandSpecies.StoredProcedure,
						   new QueryParameter("@LoanID", Result.Loan.LoanID)
					);

					// mark payment time for each schedule payment
					p.SchedulePayments.ForEach(sp => sp.PaymentDate = p.PaymentTime);

					p.FeePayments.Clear();
					p.FeePayments = DB.Fill<NL_LoanFeePayments>("NL_LoanFeePaymentsGet", CommandSpecies.StoredProcedure,
						   new QueryParameter("@LoanID", Result.Loan.LoanID)
					);
				}

				// valid rollover (StateDate between rollover expiration and creation time
				Result.Loan.AcceptedRollovers = DB.Fill<NL_LoanRollovers>("NL_AcceptedRollovers", CommandSpecies.StoredProcedure,
					   new QueryParameter("@LoanID", Result.Loan.LoanID)
					//, new QueryParameter("@Now", StateDate)
				);


				// get loan state updated by calculator
				try {
					ALoanCalculator calc = new LegacyLoanCalculator(Result);
					calc.GetState();
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