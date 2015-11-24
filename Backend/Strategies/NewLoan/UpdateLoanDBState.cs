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

		public UpdateLoanDBState(NL_Model nlmodel) {

			if (nlmodel.CustomerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, this.Error, this.Error, null);
				return;
			}

			if (nlmodel.Loan == null || nlmodel.Loan.LoanID == 0) {
				this.Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, this.Error, this.Error, null);
				return;
			}

			Model = nlmodel;

			this.strategyArgs = new object[] { nlmodel };
		} // ctor

		public override string Name { get { return "UpdateLoanDBState"; } }

		public string Error;

		public NL_Model Model { get; private set; }

		private readonly object[] strategyArgs;


		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		public override void Execute() {

			if (!string.IsNullOrEmpty(this.Error)) {
				throw new NL_ExceptionInputDataInvalid(this.Error);
			}

			int loanstatusBefore = Model.Loan.LoanStatusID;

			NL_AddLog(LogType.Error, "Loan dbState loaded", this.strategyArgs, Model, this.Error, null);

			// get loan dbState updated by calculator
			try {
				ALoanCalculator calc = new LegacyLoanCalculator(Model);
				calc.GetState();
			} catch (NoInitialDataException noInitialDataException) {
				this.Error = noInitialDataException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", Model, this.Error, this.Error, null);
			} catch (InvalidInitialInterestRateException invalidInitialInterestRateException) {
				this.Error = invalidInitialInterestRateException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", Model, this.Error, this.Error, null);
			} catch (NoLoanHistoryException noLoanHistoryException) {
				this.Error = noLoanHistoryException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", Model, this.Error, this.Error, null);
			} catch (InvalidInitialAmountException invalidInitialAmountException) {
				this.Error = invalidInitialAmountException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", Model, this.Error, this.Error, null);
			} catch (OverflowException overflowException) {
				this.Error = overflowException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", Model, this.Error, this.Error, null);
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Error = ex.Message;
				NL_AddLog(LogType.Error, "Calculator exception", Model, this.Error, this.Error, null);
			}

			NL_AddLog(LogType.Error, "Calculator's dbState loaded", this.strategyArgs, Model, this.Error, null);

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				List<NL_LoanSchedules> schedules = new List<NL_LoanSchedules>();
				List<NL_LoanSchedulePayments> schedulePayments = new List<NL_LoanSchedulePayments>();
				List<NL_LoanFeePayments> feePayments = new List<NL_LoanFeePayments>();
				List<BigintList> resetPayments = new List<BigintList>();

				// save new history - on rescheduling/rollover
				foreach (NL_LoanHistory h in Model.Loan.Histories.Where(h => h.LoanHistoryID == 0)) {
					h.LoanHistoryID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", h));
					h.Schedule.ForEach(s => s.LoanHistoryID = h.LoanHistoryID);
				}

				Model.Loan.Histories.ForEach(h => h.Schedule.ForEach(s => schedules.Add(s)));

				// assign payment to loan
				foreach (NL_Payments p in Model.Loan.Payments) {
					p.SchedulePayments.Where(sp => sp.LoanSchedulePaymentID == 0)
						.ForEach(sp => schedulePayments.Add(sp));
					p.FeePayments.Where(fp => fp.LoanFeePaymentID == 0)
						.ForEach(fp => feePayments.Add(fp));

					p.FeePayments.Where(fp => fp.LoanFeePaymentID > 0 && fp.Amount == 0).ForEach(fp => resetPayments.Add(new BigintList() { Item = fp.PaymentID }));
					p.SchedulePayments.Where(sp => sp.LoanSchedulePaymentID > 0 && sp.InterestPaid == 0 && sp.PrincipalPaid == 0).ForEach(fp => resetPayments.Add(new BigintList() { Item = fp.PaymentID }));
				}

				pconn.BeginTransaction();

				// add new schedules - on rescheduling/rollover
				DB.ExecuteNonQuery("NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", schedules.Where(s => s.LoanScheduleID == 0)));

				// update existing schedules - closed time and statuses
				foreach (NL_LoanSchedules s in schedules.Where(s => s.LoanScheduleID > 0)) {
					DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanScheduleID", s.LoanScheduleID),
							new QueryParameter("LoanScheduleStatusID", s.LoanScheduleStatusID),
							new QueryParameter("ClosedTime", s.ClosedTime)
							);
				}

				// reset after reordered/cancelled payments - their amounts
				DB.ExecuteNonQuery("NL_PaidAmountsReset", CommandSpecies.StoredProcedure, DB.CreateTableParameter<BigintList>("PaymentIds", resetPayments));
				
				// save new schedule payment
				if (schedulePayments.Count > 0) {
					DB.ExecuteNonQuery("NL_LoanSchedulePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedulePayments>("Tbl", schedulePayments));
				}
				// save new fee payments
				if (feePayments.Count > 0) {
					DB.ExecuteNonQuery("NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFeePayments>("Tbl", feePayments));
				}

				// update loan status
				if (loanstatusBefore != Model.Loan.LoanStatusID) {
					DB.ExecuteNonQuery("NL_LoanUpdate", CommandSpecies.StoredProcedure,
						new QueryParameter("LoanID", Model.Loan.LoanID),
						new QueryParameter("LoanStatusID", Model.Loan.LoanStatusID),
						new QueryParameter("DateClosed", Model.Loan.DateClosed)
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

	public class BigintList {
		public long Item { get; set; }
	}


}