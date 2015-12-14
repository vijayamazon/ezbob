namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;
	using Newtonsoft.Json.Linq;
	using NHibernate.Linq;

	/// <summary>
	/// triggered on payment adding/cancellation, rollover, re-scheduling.
	/// Save recalculated loan state differenced to DB
	/// </summary>
    public class UpdateLoanDBState : AStrategy, Inlstrategy {

		public UpdateLoanDBState(int customerID, long loanID, int userID) {

			this.strategyArgs = new object[] { customerID, loanID, userID };

			CustomerID = customerID;
			LoanID = loanID;
			UserID = userID;
		} // ctor

		public override string Name { get { return "UpdateLoanDBState"; } }

		public string Error { get; private set; }

		public int CustomerID { get; private set; }
		public int UserID { get; private set; }

		public long LoanID { get; private set; }

		private object[] strategyArgs;

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		public override void Execute() {

            if (!IsNewLoanRunStrategy)
                return;

            NL_AddLog(LogType.Info, "Strategy Start", LoanID, null, null, null);
			NL_AddLog(LogType.Error, "Started", this.strategyArgs, Error, null, null);
			if (CustomerID == 0) {
				Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionCustomerNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionCustomerNotFound(Error);
			}

			if (LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionLoanNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionLoanNotFound(Error);
			}

			// get raw DB state of the loan - without calc
			GetLoanState state = new GetLoanState(CustomerID, LoanID, DateTime.UtcNow, Context.UserID, false);
			state.Execute();

			// failed to load loan from DB
			if (!string.IsNullOrEmpty(state.Error)) {
				Error = state.Error;
				NL_AddLog(LogType.Error, "Loan get state failed", this.strategyArgs, state.Error, Error, null);
				return;
			}

			this.strategyArgs = new object[] { CustomerID, LoanID, UserID };

			var stateBefore = JObject.FromObject(state.Result.Loan);

			NL_Model RecalculatedModel = new NL_Model(state.Result.CustomerID);
			RecalculatedModel.Loan = state.Result.Loan;

			// get loan state updated by calculator
			try {
				ALoanCalculator calc = new LegacyLoanCalculator(RecalculatedModel);
				calc.GetState();
			} catch (NoInitialDataException noInitialDataException) {
				Error = noInitialDataException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, RecalculatedModel, Error, null);
			} catch (InvalidInitialInterestRateException invalidInitialInterestRateException) {
				Error = invalidInitialInterestRateException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, RecalculatedModel, Error, null);
			} catch (NoLoanHistoryException noLoanHistoryException) {
				Error = noLoanHistoryException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, RecalculatedModel, Error, null);
			} catch (InvalidInitialAmountException invalidInitialAmountException) {
				Error = invalidInitialAmountException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, RecalculatedModel, Error, null);
			} catch (OverflowException overflowException) {
				Error = overflowException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, RecalculatedModel, Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, RecalculatedModel, Error, null);
				return;
			}

			// no changes, exit
			var stateAfter = JObject.FromObject(RecalculatedModel.Loan);
			if (JToken.DeepEquals(stateBefore, stateAfter)) {
				NL_AddLog(LogType.Info, "End - no diff btwn DB state and recalculated state", this.strategyArgs, RecalculatedModel, Error, null);
				return;
			}

			NL_AddLog(LogType.Info, "recalculated loan state", this.strategyArgs, RecalculatedModel, Error, null);

			List<NL_LoanSchedules> schedules = new List<NL_LoanSchedules>();
			List<NL_LoanSchedulePayments> schedulePayments = new List<NL_LoanSchedulePayments>();
			List<NL_LoanFeePayments> feePayments = new List<NL_LoanFeePayments>();

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				// save new history - on rescheduling/rollover
				foreach (NL_LoanHistory h in RecalculatedModel.Loan.Histories.Where(h => h.LoanHistoryID == 0)) {
					h.LoanHistoryID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", h));
					// set for newly created scheduled it historyID
					h.Schedule.ForEach(s => s.LoanHistoryID = h.LoanHistoryID);
				}

				// collect all schedules into one list
				RecalculatedModel.Loan.Histories.ForEach(h => h.Schedule.ForEach(s => schedules.Add(s)));

				// save new schedules - on rescheduling/rollover
				DB.ExecuteNonQuery("NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", schedules.Where(s => s.LoanScheduleID == 0)));

				// update schedules - closed time and statuses
				foreach (NL_LoanSchedules s in schedules) {
					DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanScheduleID", s.LoanScheduleID),
							new QueryParameter("LoanScheduleStatusID", s.LoanScheduleStatusID),
							new QueryParameter("ClosedTime", s.ClosedTime));
				}

				// assign payment to loan
				foreach (NL_Payments p in RecalculatedModel.Loan.Payments) {

					// new SchedulePayments
					p.SchedulePayments.Where(sp => sp.NewEntry).ForEach(sp => schedulePayments.Add(sp));

					// update existing schedule payments - TODO remove after development end
					p.SchedulePayments.Where(sp => !sp.NewEntry).ForEach(sp =>
						DB.ExecuteNonQuery("NL_LoanSchedulePaymentsUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanSchedulePaymentID", sp.LoanSchedulePaymentID),
							new QueryParameter("PrincipalPaid", sp.PrincipalPaid),
							new QueryParameter("InterestPaid", sp.InterestPaid)));

					// new FeePayments
					p.FeePayments.Where(fp => fp.NewEntry).ForEach(fp => feePayments.Add(fp));

					// update existing fee payments - TODO remove after development end
					p.FeePayments.Where(fp => !fp.NewEntry).ForEach(fp =>
						DB.ExecuteNonQuery("NL_LoanFeePaymentsUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanFeePaymentID", fp.LoanFeePaymentID),
							new QueryParameter("Amount", fp.Amount))
						);
				}
			
				// save new schedule payment
				if (schedulePayments.Count > 0) {
					DB.ExecuteNonQuery("NL_LoanSchedulePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedulePayments>("Tbl", schedulePayments));
				}

				// save new fee payments
				if (feePayments.Count > 0) {
					DB.ExecuteNonQuery("NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFeePayments>("Tbl", feePayments));
				}

				// update loan status
				if (RecalculatedModel.Loan.LoanStatusID != state.Result.Loan.LoanStatusID) {
					DB.ExecuteNonQuery("NL_LoanUpdate", CommandSpecies.StoredProcedure,
						new QueryParameter("LoanID", RecalculatedModel.Loan.LoanID),
						new QueryParameter("LoanStatusID", RecalculatedModel.Loan.LoanStatusID),
						new QueryParameter("DateClosed", RecalculatedModel.Loan.DateClosed)
						);
				}

				pconn.Commit();

                NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, RecalculatedModel, Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();

				Error = ex.Message;
				Log.Error("Failed to update loan DB dbState. err: {0}", Error);

				NL_AddLog(LogType.Error, "Failed - Rollback", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
			}
		}
	}

	public class BigintList {
		public long Item { get; set; }
	}
}