namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	public class AddPayment : AStrategy {

		public AddPayment(NL_Payments payment) {

			if (Context.CustomerID == null || Context.CustomerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			CustomerID = (int)Context.CustomerID;

			Payment = payment;

			this.strategyArgs = new object[] { CustomerID, Context.UserID, Payment };
		}

		public override string Name { get { return "AddPayment"; } }
		public NL_Payments Payment { get; private set; }
		public int CustomerID { get; private set; }
		public string Error;
		public long PaymentID { get; private set; }
		private readonly object[] strategyArgs;

		public override void Execute() {

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				PaymentID = DB.ExecuteScalar<long>("NL_PaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Payments>("Tbl", Payment));
				Payment.PaymentID = PaymentID;

				if (Payment.PaypointTransactions.Count > 0) {

					NL_PaypointTransactions ppTransaction = Payment.PaypointTransactions.FirstOrDefault();

					if (ppTransaction == null) {
						Log.Info("Paypoint transaction not found. payment {0}", Payment);
					} else {

						ppTransaction.PaymentID = Payment.PaymentID;
						ppTransaction.PaypointTransactionID = DB.ExecuteScalar<long>("NL_PaypointTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_PaypointTransactions>("Tbl", ppTransaction));
					}
				}

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();

				this.Error = ex.Message;
				Log.Error("Failed to add new payment: {0}", this.Error);

				NL_AddLog(LogType.Error, "Strategy Faild - Rollback", Payment, this.Error, ex.ToString(), ex.StackTrace);

				return;
			}

			// TODO call UpdateLoanState
			UpdateLoanState updateLoanStrategy = new UpdateLoanState(Payment.LoanID);
			updateLoanStrategy.Context.CustomerID = CustomerID;
			updateLoanStrategy.Context.UserID = Context.UserID;

			/*// assign payment to loan

			// get DB state
			NL_Model model = new NL_Model(CustomerID) { UserID = Context.UserID };
			LoanState loanState = new LoanState(model, Payment.LoanID, DateTime.UtcNow);
			loanState.Execute();
			model = loanState.Result;

			// get loan state updated
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
			} catch (Exception ex) {
				this.Error = ex.Message;
				NL_AddLog(LogType.Error, "Calculator exception", model, this.Error, this.Error, null);
			}

			List<NL_LoanSchedules> newSchedules = new List<NL_LoanSchedules>();
			List<NL_LoanSchedulePayments> schedulePayments = new List<NL_LoanSchedulePayments>();
			List<NL_LoanFeePayments> feePayments = new List<NL_LoanFeePayments>();

			foreach (NL_LoanHistory h in model.Loan.Histories) {
				foreach (NL_LoanSchedules s in h.Schedule) {
					if (s.LoanScheduleID == 0) {
						newSchedules.Add(s);
					} else {
						// update existing schedules
						DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanScheduleID", s.LoanScheduleID),
							new QueryParameter("LoanScheduleStatusID", s.LoanScheduleStatusID),
							new QueryParameter("ClosedTime", s.ClosedTime)
							);
					}
				}
			}

			// addd new schedules
			DB.ExecuteNonQuery("NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", newSchedules));

			

			model.Loan.Payments.ForEach(p => p.SchedulePayments.ForEach(sp => schedulePayments.Add(sp)));

			List<NL_LoanSchedulePayments> schPayments = (List<NL_LoanSchedulePayments>)schedulePayments.Where(sp => sp.LoanSchedulePaymentID == 0);
			
			if (schPayments.Count > 0) {
				DB.ExecuteNonQuery("NL_LoanSchedulePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedulePayments>("Tbl", schPayments));
			}

			// existing schedule payments
			schPayments = (List<NL_LoanSchedulePayments>)schedulePayments.Where(sp => sp.LoanSchedulePaymentID > 0);

			if (schPayments.Count > 0) {
				foreach (NL_LoanSchedulePayments sp in schPayments) {
					DB.ExecuteNonQuery("NL_LoanSchedulePaymentsUpdate", CommandSpecies.StoredProcedure,
								new QueryParameter("LoanSchedulePaymentID", sp.LoanSchedulePaymentID),
							//	new QueryParameter("LoanScheduleID", sp.LoanScheduleID),
							//	new QueryParameter("PaymentID", sp.PaymentID),
								new QueryParameter("PrincipalPaid", sp.PrincipalPaid),
								new QueryParameter("InterestPaid", sp.InterestPaid)
							);
				}
			}

			model.Loan.Payments.ForEach(p => p.FeePayments.ForEach(fp => feePayments.Add(fp)));

			// new fee payments
			List<NL_LoanFeePayments> fPayments = (List<NL_LoanFeePayments>)feePayments.Where(fp => fp.LoanFeePaymentID == 0);

			if (fPayments.Count > 0) {
				DB.ExecuteNonQuery("NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFeePayments>("Tbl", fPayments));
			}

			fPayments = (List<NL_LoanFeePayments>)feePayments.Where(fp => fp.LoanFeePaymentID > 0);
			if (fPayments.Count > 0) {
				foreach (NL_LoanFeePayments fp in fPayments) {
					DB.ExecuteNonQuery("NL_LoanFeePaymentsUpdate", CommandSpecies.StoredProcedure,
						new QueryParameter("LoanFeePaymentID", fp.LoanFeePaymentID),
					//	new QueryParameter("PaymentID", fp.PaymentID),
						new QueryParameter("Amount", fp.Amount)
					);
				}
			}*/
		}
	} // class AddPayment
} // ns