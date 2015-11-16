namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using NHibernate.Linq;

	public class AddPayment : AStrategy {

		public AddPayment(int customerID, NL_Payments payment) {
			CustomerID = customerID;
			Payment = payment;
		}

		public override string Name { get { return "AddPayment"; } }
		public NL_Payments Payment { get; private set; }
		public int CustomerID { get; private set; }
		public string Error;

		public override void Execute() {

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				Payment.PaymentID = DB.ExecuteScalar<long>("NL_PaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Payments>("Tbl", Payment));

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


			// assign payment to loan

			// get DB state
			NL_Model model = new NL_Model(CustomerID) { UserID = Context.UserID };
			LoanState loanState = new LoanState(model, Payment.LoanID, DateTime.UtcNow);
			loanState.Execute();
			model = loanState.Result;

			List<NL_LoanSchedulePayments> schedulePayments = new List<NL_LoanSchedulePayments>();
			List<NL_LoanFeePayments> feePayments = new List<NL_LoanFeePayments>();

			foreach (NL_Payments p in model.Loan.Payments) {
				p.SchedulePayments.Where(sp => sp.PaymentID == Payment.PaymentID).ForEach(sp => schedulePayments.Add(sp));
				p.FeePayments.Where(fp => fp.PaymentID == Payment.PaymentID).ForEach(fp => feePayments.Add(fp));
			}

			// record schedule/fees payments
			if (schedulePayments.Count > 0) {
				DB.ExecuteNonQuery("NL_LoanSchedulePaymentsList", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedulePayments>("Tbl", schedulePayments));
			}

			if (feePayments.Count > 0) {
				DB.ExecuteNonQuery("NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFeePayments>("Tbl", feePayments));
			}
		}
	} // class AddPayment
} // ns