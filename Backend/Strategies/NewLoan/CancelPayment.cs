namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	/// <summary>
	/// https://ezbobtech.atlassian.net/browse/EZ-4390
	/// 
	/// Update: 
	/// [PaymentStatusID] - ChargeBack or WrongPayment
	/// [DeletionTime]
	/// [DeletedByUserID]
	/// [Notes]
	/// 
	/// By PaymentID
	/// TODO: Add file uploading on full UI implementation of https://ezbobtech.atlassian.net/browse/EZ-3452
	/// file uploading - not supported yet
	/// </summary>
	public class CancelPayment : AStrategy {

		public CancelPayment(int customerID, NL_Payments payment, int userID) {
			CustomerID = customerID;
			UserID = userID;
			Payment = payment;
			this.strategyArgs = new object[] { CustomerID, UserID, Payment };
		}

		public override string Name { get { return "CancelPayment"; } }

		public NL_Payments Payment { get; private set; }
		public int CustomerID { get; private set; }
		public int UserID { get; private set; }
		public string Error { get; private set; }
		private readonly object[] strategyArgs;

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Started", this.strategyArgs, Error, null, null);

			if (CustomerID == 0) {
				Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionCustomerNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionCustomerNotFound(Error);
			}

			if (Payment == null || Payment.LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionLoanNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionLoanNotFound(Error);
			}

			if (Payment.PaymentID == 0) {
				Error = NL_ExceptionInputDataInvalid.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionInputDataInvalid.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionInputDataInvalid(Error);
			}

			if (Payment.PaymentStatusID != (int)NLPaymentStatuses.ChargeBack && Payment.PaymentStatusID != (int)NLPaymentStatuses.WrongPayment) {
				Error = "PaymentStatusID not ChargeBack and not WrongPayment";
				NL_AddLog(LogType.Error, NL_ExceptionInputDataInvalid.DefaultMessage, this.strategyArgs, Error, null, null);
				throw new NL_ExceptionInputDataInvalid(Error);
			}

			if (Payment.DeletionTime.Equals(DateTime.MinValue) || Payment.DeletionTime == null) {
				Error = "DeletionTime not set";
				NL_AddLog(LogType.Error, NL_ExceptionInputDataInvalid.DefaultMessage, this.strategyArgs, Error, null, null);
				throw new NL_ExceptionInputDataInvalid(Error);
			}

			if (Payment.DeletedByUserID == null || Payment.DeletedByUserID == 0) {
				Error = "DeletedByUserID not set";
				NL_AddLog(LogType.Error, NL_ExceptionInputDataInvalid.DefaultMessage, this.strategyArgs, Error, null, null);
				throw new NL_ExceptionInputDataInvalid(Error);
			}

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				Log.Debug("==============================={0}", new QueryParameter("PaymentStatusID", Payment.PaymentStatusID));

				// RESET PAID PRINCIPAL, INTEREST (SCHEDULE), FEES PAID AFTER [DeletionTime] on delete payment  - in SP NL_ResetPaymentsPaidAmounts, called from NL_PaymentCancel
				DB.ExecuteNonQuery(pconn, "NL_PaymentCancel", CommandSpecies.StoredProcedure, 
					new QueryParameter("PaymentID", Payment.PaymentID),
					new QueryParameter("LoanID", Payment.LoanID),
					new QueryParameter("PaymentStatusID", Payment.PaymentStatusID),
					new QueryParameter("DeletionTime", Payment.DeletionTime),
					new QueryParameter("DeletedByUserID", Payment.DeletedByUserID),
					new QueryParameter("Notes", Payment.Notes)
				);
				
				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();

				Error = ex.Message;
				Log.Error("Failed to cancel payment: {0}", Error);

				NL_AddLog(LogType.Error, "Failed - Rollback", Payment, Error, ex.ToString(), ex.StackTrace);

				return;
			}

			NL_AddLog(LogType.Info, "End", this.strategyArgs, Payment, null, null);

			// recalculate state with calculator + save new state to DB
			UpdateLoanDBState reloadLoanDBState = new UpdateLoanDBState(CustomerID, Payment.LoanID, UserID);
			try {
				reloadLoanDBState.Execute();
			} catch (Exception ex) {
				Error = ex.Message;
				NL_AddLog(LogType.Error, "Failed on UpdateLoanDBState", Payment, reloadLoanDBState.Error + "\n" + Error, ex.ToString(), ex.StackTrace);
			}

		}
	} // class AddPayment
} // ns