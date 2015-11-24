namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	public class AddPayment : AStrategy {

		public AddPayment(int customerID, NL_Payments payment) {

			this.strategyArgs = new object[] { customerID, payment };

			if (customerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			if (payment == null || payment.LoanID == 0) {
				this.Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			CustomerID = customerID;
			Payment = payment;

			this.strategyArgs = new object[] { CustomerID, Payment };
		}

		public override string Name { get { return "AddPayment"; } }
		public NL_Payments Payment { get; private set; }
		public int CustomerID { get; private set; }
	
		public string Error;
		public long PaymentID { get; private set; }

		private readonly object[] strategyArgs;

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		public override void Execute() {

			if (!string.IsNullOrEmpty(this.Error)) {
				throw new NL_ExceptionInputDataInvalid(this.Error);
			}

			NL_AddLog(LogType.Info, "Started", this.strategyArgs, this.Error, null, null);

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				PaymentID = DB.ExecuteScalar<long>("NL_PaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Payments>("Tbl", Payment));
				Payment.PaymentID = PaymentID;

				if (Payment.PaypointTransactions.Count > 0) {

					NL_PaypointTransactions ppTransaction = Payment.PaypointTransactions.FirstOrDefault();

					if (ppTransaction == null) {
						Log.Info("Paypoint transaction not found. Payment \n{0}{1}", AStringable.PrintHeadersLine(typeof(NL_Payments)), Payment.ToStringAsTable());
						NL_AddLog(LogType.Info, "Paypoint transaction not found", this.strategyArgs, this.Error, null, null);
					} else {
						ppTransaction.PaymentID = Payment.PaymentID;
						ppTransaction.PaypointTransactionID = DB.ExecuteScalar<long>("NL_PaypointTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_PaypointTransactions>("Tbl", ppTransaction));
					}
				}

				// RESET ALL PAID PRINCIPAL, INTEREST (SCHEDULE), FEES PAID AFTER [DeletionTime] of deleted payment. New distribution of paid p, i, f (s) will be recalculated and saved again
				/*if (Payment.PaymentStatusID == (int)NLPaymentStatuses.Cancelled) {
					DB.ExecuteNonQuery("NL_CancelledPaymentPaidAmountsReset", CommandSpecies.StoredProcedure, new QueryParameter("PaymentID", PaymentID));
				}*/

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();

				this.Error = ex.Message;
				Log.Error("Failed to add new payment: {0}", this.Error);

				NL_AddLog(LogType.Error, "Strategy Faild - Rollback", Payment, this.Error, ex.ToString(), ex.StackTrace);

				return;
			}

			// get DB State 

			/*GetLoanState getLoanState = new GetLoanState(CustomerID, Payment.LoanID, DateTime.UtcNow, Context.UserID, false);
			getLoanState.Execute();

			// failed to load loan from DB
			if (!string.IsNullOrEmpty(getLoanState.Error)) {
				this.Error = getLoanState.Error;
				NL_AddLog(LogType.Error, "Loan get state failed", this.strategyArgs, getLoanState.Error, this.Error, null);
				return;
			}*/

			// recalculate state by calculator + save new state to DB
			UpdateLoanDBState reloadLoanDBState = new UpdateLoanDBState(CustomerID, Payment.LoanID);
			reloadLoanDBState.Context.CustomerID = CustomerID;
			reloadLoanDBState.Context.UserID = Context.UserID;
			reloadLoanDBState.Execute();

		}
	} // class AddPayment
} // ns