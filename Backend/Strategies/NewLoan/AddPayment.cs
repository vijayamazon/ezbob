namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.DAL;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	public class AddPayment : AStrategy {

		public AddPayment(int customerID, NL_Payments payment, int userID) {

			this.strategyArgs = new object[] { customerID, payment, userID };

			if (customerID == 0) {
				Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			if (payment == null || payment.LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			CustomerID = customerID;
			Payment = payment;
			UserID = userID;

			this.strategyArgs = new object[] { CustomerID, Payment, UserID };
		}

		public override string Name { get { return "AddPayment"; } }
		public NL_Payments Payment { get; private set; }
		public int CustomerID { get; private set; }
		public int UserID { get; private set; }

		public string Error { get; private set; }
		public long PaymentID { get; private set; }

		private readonly object[] strategyArgs;

		//[SetterProperty]
		public ILoanDAL LoanDAL { get; set; }

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		public override void Execute() {

			// invalid input
			if (!string.IsNullOrEmpty(Error)) {
				throw new NL_ExceptionInputDataInvalid(Error);
			}

			NL_AddLog(LogType.Info, "Started", this.strategyArgs, Error, null, null);

			// load loan
			var loan = LoanDAL.GetLoan(Payment.LoanID);

			if (loan.LoanStatusID == (int)NLLoanStatuses.Pending) {
				// loan pending - can't to add payment
				Error = string.Format("Loan {0} in status 'Pending' yet, payment registering not allowed.", loan.LoanID);
				Log.Debug(Error);
				NL_AddLog(LogType.Info, "End", this.strategyArgs, Error, Error, null);
				return;
			}

			if ((loan.LoanStatusID == (int)NLLoanStatuses.PaidOff || loan.LoanStatusID == (int)NLLoanStatuses.WriteOff) && loan.DateClosed!=null) {
				// loan closed - can't to add payment
				Error = string.Format("Loan {0} in status {1} since {2:d}, payment registering not allowed.", loan.LoanID, loan.LoanStatusID, loan.DateClosed);
				Log.Debug(Error);
				NL_AddLog(LogType.Info, "End", this.strategyArgs, Error, Error, null);
				return;
			}

			// TODO check adding the payment twice  kak?
			/*if (state.Result.Loan.Payments.FirstOrDefault(p=>p.CreationTime.Equals(Payment.CreationTime) {}*/

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				//  RESET PAID PRINCIPAL, INTEREST (SCHEDULE), FEES PAID on retroactive payment - in SP NL_ResetPaymentsPaidAmounts, called from NL_PaymentsSave.
				PaymentID = DB.ExecuteScalar<long>("NL_PaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Payments>("Tbl", Payment));
				Payment.PaymentID = PaymentID;

				if (Payment.PaypointTransactions.Count > 0) {

					NL_PaypointTransactions ppTransaction = Payment.PaypointTransactions.FirstOrDefault();

					if (ppTransaction == null) {
						Log.Info("Paypoint transaction not found. Payment \n{0}{1}", AStringable.PrintHeadersLine(typeof(NL_Payments)), Payment.ToStringAsTable());
						NL_AddLog(LogType.Info, "Paypoint transaction not found", this.strategyArgs, Error, null, null);
					} else {
						ppTransaction.PaymentID = Payment.PaymentID;
						ppTransaction.PaypointTransactionID = DB.ExecuteScalar<long>("NL_PaypointTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_PaypointTransactions>("Tbl", ppTransaction));
					}
				}

				pconn.Commit();

				NL_AddLog(LogType.Info, "End", this.strategyArgs, Payment, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();

				Error = ex.Message;
				Log.Error("Failed to add new payment: {0}", Error);
				NL_AddLog(LogType.Error, "Strategy Faild - Rollback", Payment, Error, ex.ToString(), ex.StackTrace);

				return;
			}

			// recalculate state by calculator + save new state to DB
			UpdateLoanDBState reloadLoanDBState = new UpdateLoanDBState(CustomerID, Payment.LoanID, UserID);
			try {
				reloadLoanDBState.Execute();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				Log.Error("Failed on UpdateLoanDBState {0}", Error);
				NL_AddLog(LogType.Error, "Failed on UpdateLoanDBState", Payment, reloadLoanDBState.Error + "\n" + Error, ex.ToString(), ex.StackTrace);
			}

		}
	} // class AddPayment
} // ns