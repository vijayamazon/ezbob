namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;
	using NHibernate.Linq;

	/// <summary>
	/// triggers for this strategy: payment cancellation (done); payment adding (done); rollover; re-scheduling;
	/// </summary>
	public class UpdateLoanDBState : AStrategy {

		public UpdateLoanDBState(long loanID) {

			if (Context.CustomerID == null || Context.CustomerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, this.Error, this.Error, null);
				return;
			}

			if (Context.CustomerID != null)
				CustomerID = (int)Context.CustomerID;

			LoanID = loanID;

			this.strategyArgs = new object[] { CustomerID, Context.UserID, LoanID };
		}

		public override string Name { get { return "UpdateLoanState"; } }
		public int CustomerID { get; private set; }
		public string Error;
		public long LoanID { get; private set; }
		
		private readonly object[] strategyArgs;
	
		public override void Execute() {

			// get DB dbState before
			NL_Model model = new NL_Model(CustomerID) { UserID = Context.UserID };
			GetLoanDBState getLoanDBState = new GetLoanDBState(model, LoanID, DateTime.UtcNow);
			getLoanDBState.Execute();

			// failed to loan loan from DB
			if (!string.IsNullOrEmpty(getLoanDBState.Error)) {
				this.Error = getLoanDBState.Error;
				NL_AddLog(LogType.Error, "Loan dbState failed", this.strategyArgs, getLoanDBState.Error, this.Error, null);
				return;
			}

			model = getLoanDBState.Result;

			int loanstatusBefore = model.Loan.LoanStatusID;

			NL_AddLog(LogType.Error, "Loan dbState loaded", this.strategyArgs, model, this.Error, null);

			// get loan dbState updated by calculator
			try {
				ALoanCalculator calc = new LegacyLoanCalculator(model);
				calc.GetState();
			} catch (NoInitialDataException noInitialDataException) {
				this.Error = noInitialDataException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", model, this.Error, this.Error, null);
			} catch (InvalidInitialInterestRateException invalidInitialInterestRateException) {
				this.Error = invalidInitialInterestRateException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", model, this.Error, this.Error, null);
			} catch (NoLoanHistoryException noLoanHistoryException) {
				this.Error = noLoanHistoryException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", model, this.Error, this.Error, null);
			} catch (InvalidInitialAmountException invalidInitialAmountException) {
				this.Error = invalidInitialAmountException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", model, this.Error, this.Error, null);
			} catch (OverflowException overflowException) {
				this.Error = overflowException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", model, this.Error, this.Error, null);
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Error = ex.Message;
				NL_AddLog(LogType.Error, "Calculator exception", model, this.Error, this.Error, null);
			}

			NL_AddLog(LogType.Error, "Calculator's dbState loaded", this.strategyArgs, model, this.Error, null);

			ConnectionWrapper pconn = DB.GetPersistent();

			try {
				
				List<NL_LoanSchedules> schedules = new List<NL_LoanSchedules>();
				List<NL_LoanSchedulePayments> schedulePayments = new List<NL_LoanSchedulePayments>();
				List<NL_LoanFeePayments> feePayments = new List<NL_LoanFeePayments>();

				pconn.BeginTransaction();

				// save new history (on rescheduling/rollover)
				foreach (NL_LoanHistory h in model.Loan.Histories.Where(h => h.LoanHistoryID == 0)) {
					h.LoanHistoryID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", h));
					h.Schedule.ForEach(s => s.LoanHistoryID = h.LoanHistoryID);
				}

				model.Loan.Histories.ForEach(h => h.Schedule.ForEach(s => schedules.Add(s)));

				// add new schedules - when may happen?
				DB.ExecuteNonQuery("NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", schedules.Where(s=>s.LoanScheduleID == 0)));

				// update existing schedules - closed time and status
				foreach (NL_LoanSchedules s in schedules.Where(s => s.LoanScheduleID > 0)) {
					DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanScheduleID", s.LoanScheduleID),
							new QueryParameter("LoanScheduleStatusID", s.LoanScheduleStatusID),
							new QueryParameter("ClosedTime", s.ClosedTime)
							);
				}

				// assign payment to loan

				// new schedule payments
				model.Loan.Payments.ForEach(p => p.SchedulePayments.Where(sp => sp.LoanSchedulePaymentID == 0 ).ForEach(sp => schedulePayments.Add(sp)));
	
				if (schedulePayments.Count > 0) {
					DB.ExecuteNonQuery("NL_LoanSchedulePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedulePayments>("Tbl", schedulePayments));
				}

				// new fee payments
				model.Loan.Payments.ForEach(p => p.FeePayments.Where(fp=>fp.LoanFeePaymentID == 0).ForEach(fp => feePayments.Add(fp)));

				if (feePayments.Count > 0) {
					DB.ExecuteNonQuery("NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFeePayments>("Tbl", feePayments));
				}

				// update loan status
				if (loanstatusBefore != model.Loan.LoanStatusID) {
					DB.ExecuteNonQuery("NL_LoanUpdate", CommandSpecies.StoredProcedure, 
						new QueryParameter("LoanID", LoanID ),
						new QueryParameter("LoanStatusID", model.Loan.LoanStatusID ),
						new QueryParameter("DateClosed", model.Loan.DateClosed)
						);
				}

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();

				this.Error = ex.Message;
				Log.Error("Failed to update loan DB dbState. err: {0}", this.Error);

				NL_AddLog(LogType.Error, "Faild - Rollback", this.strategyArgs, this.Error, ex.ToString(), ex.StackTrace);
			}
		}
	} 
} 