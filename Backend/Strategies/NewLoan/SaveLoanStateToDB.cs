namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;
	using NHibernate.Linq;

	internal class SaveLoanStateToDB : AStrategy {

		internal SaveLoanStateToDB(NL_Model nlmodel, bool loanClose = false, bool loanStatusChange = false) {
			model = nlmodel;
			runClose = loanClose;
			runStatusChenge = loanStatusChange;
			this.strategyArgs = new object[] { model };
		} // ctor

		public override string Name { get { return "SaveLoanStateToDB"; } }

		public bool runClose { get; private set; }
		public bool runStatusChenge { get; private set; }
		public NL_Model model { get; private set; }
		private readonly object[] strategyArgs;
		public string Error { get; private set; }

		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		public override void Execute() {

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, null, Error, null);

			if (model.CustomerID == 0) {
				Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionCustomerNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionCustomerNotFound(Error);
			}

			if (model.Loan.LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionLoanNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionLoanNotFound(Error);
			}

			NL_AddLog(LogType.Info, "recalculated loan state", model, null, Error, null);

			List<NL_LoanSchedules> schedules = new List<NL_LoanSchedules>();
			List<NL_LoanSchedulePayments> schedulePayments = new List<NL_LoanSchedulePayments>();
			List<NL_LoanFeePayments> feePayments = new List<NL_LoanFeePayments>();

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				// save new history - on rescheduling/rollover
				foreach (NL_LoanHistory h in model.Loan.Histories.Where(h => h.LoanHistoryID == 0)) {
					h.LoanHistoryID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", h));
					// set for newly created scheduled it historyID
					h.Schedule.ForEach(s => s.LoanHistoryID = h.LoanHistoryID);
				}

				// save up-to-date outstanding princpal, outstanding late fees, outstanding distributed fees, outstanding accumulated interest
				foreach (NL_LoanHistory h in model.Loan.Histories.Where(h => h.LoanHistoryID > 0)) {
					DB.ExecuteNonQuery(pconn, "NL_LoanHistoryUpdate", CommandSpecies.StoredProcedure,
						new QueryParameter("LoanHistoryID", h.LoanHistoryID),
						new QueryParameter("LateFees", h.LoanHistoryID),
						new QueryParameter("DistributedFees", h.LoanHistoryID),
						new QueryParameter("AccumulatedInterest", h.LoanHistoryID));
				}

				// collect all schedules into one list
				model.Loan.Histories.ForEach(h => h.Schedule.ForEach(s => schedules.Add(s)));

				// save new schedules - on rescheduling/rollover
				DB.ExecuteNonQuery(pconn, "NL_LoanSchedulesSave", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<NL_LoanSchedules>("Tbl", schedules.Where(s => s.LoanScheduleID == 0)));

				// update schedules - closed time and statuses
				foreach (NL_LoanSchedules s in schedules.Where(s => s.LoanScheduleID > 0)) {
					DB.ExecuteNonQuery(pconn, "NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanScheduleID", s.LoanScheduleID),
							new QueryParameter("LoanScheduleStatusID", s.LoanScheduleStatusID),
							new QueryParameter("ClosedTime", s.ClosedTime));
				}

				// disable fees
				//foreach (NL_LoanFees f in model.Loan.Fees.Where(f => f.LoanFeeID > 0 && f.DeletedByUserID != null && f.DisabledTime != null)) {
				//	DB.ExecuteNonQuery(pconn, "NL_LoanFeeCancel", CommandSpecies.StoredProcedure,
				//			new QueryParameter("LoanFeeID", f.LoanFeeID),
				//			new QueryParameter("DeletedByUserID", f.DeletedByUserID),
				//			new QueryParameter("DisabledTime", f.DisabledTime),
				//			new QueryParameter("Notes", f.Notes));
				//}

				// insert fees
				DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<NL_LoanFees>("Tbl", model.Loan.Fees.Where(f => f.LoanFeeID == 0)));

				// assign payment to loan
				foreach (NL_Payments p in model.Loan.Payments) {

					// new SchedulePayments
					p.SchedulePayments.Where(sp => sp.NewEntry).ForEach(sp => schedulePayments.Add(sp));

					// update existing schedule payments - TODO remove after development end
					p.SchedulePayments.Where(sp => !sp.NewEntry).ForEach(sp =>
						DB.ExecuteNonQuery(pconn, "NL_LoanSchedulePaymentsUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanSchedulePaymentID", sp.LoanSchedulePaymentID),
							new QueryParameter("PrincipalPaid", sp.PrincipalPaid),
							new QueryParameter("InterestPaid", sp.InterestPaid)));

					// new FeePayments
					p.FeePayments.Where(fp => fp.NewEntry).ForEach(fp => feePayments.Add(fp));

					// update existing fee payments - TODO remove after development end
					p.FeePayments.Where(fp => !fp.NewEntry).ForEach(fp =>
						DB.ExecuteNonQuery(pconn, "NL_LoanFeePaymentsUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanFeePaymentID", fp.LoanFeePaymentID),
							new QueryParameter("Amount", fp.Amount))
						);
				}

				// save new schedule payment
				if (schedulePayments.Count > 0) {
					DB.ExecuteNonQuery(pconn, "NL_LoanSchedulePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedulePayments>("Tbl", schedulePayments));
				}

				// save new fee payments
				if (feePayments.Count > 0) {
					DB.ExecuteNonQuery(pconn, "NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFeePayments>("Tbl", feePayments));
				}
	
				// update loan status	
				if (runClose) {
					DB.ExecuteNonQuery(pconn, "NL_LoanUpdate", CommandSpecies.StoredProcedure,
						new QueryParameter("LoanID", model.Loan.LoanID),
						new QueryParameter("LoanStatusID", model.Loan.LoanStatusID),
						new QueryParameter("DateClosed", model.Loan.DateClosed)
						);
				}

				pconn.Commit();

				NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, model, Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();

				Error = ex.Message;
				Log.Error("Failed to update loan DB dbState. err: {0}", Error);

				NL_AddLog(LogType.Error, "Failed - Rollback", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
			}
		}
	}
}