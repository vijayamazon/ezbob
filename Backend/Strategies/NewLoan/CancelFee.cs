namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;

	/// <summary>
	/// </summary>
	public class CancelFee : AStrategy {

		public CancelFee(int customerID, NL_LoanFees fee, int userID) {
	
			CustomerID = customerID;
			Fee = fee;
			UserID = userID;

			this.strategyArgs = new object[] { CustomerID, Fee, UserID };
		}

		public override string Name { get { return "CancelPayment"; } }
		public NL_LoanFees Fee { get; private set; }
		public int CustomerID { get; private set; }
		public int UserID { get; private set; }

		public string Error;

		private readonly object[] strategyArgs;

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		public override void Execute() {

			NL_AddLog(LogType.Info, "Started", this.strategyArgs, this.Error, null, null);

			if (CustomerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionCustomerNotFound.DefaultMessage, this.strategyArgs, null, this.Error, null);
				throw new NL_ExceptionCustomerNotFound(this.Error);
			}

			if (Fee == null || Fee.LoanID == 0) {
				this.Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionLoanNotFound.DefaultMessage, this.strategyArgs, null, this.Error, null);
				throw new NL_ExceptionLoanNotFound(this.Error);
			}

			if (Fee.LoanFeeID == 0) {
				this.Error = NL_ExceptionInputDataInvalid.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionInputDataInvalid.DefaultMessage, this.strategyArgs, null, this.Error, null);
				throw new NL_ExceptionInputDataInvalid(this.Error);
			}
		
			// make fee cancellation

			// recalculate state with calculator + save new state to DB
			UpdateLoanDBState reloadLoanDBState = new UpdateLoanDBState(CustomerID, Fee.LoanID, UserID);
			try {
				reloadLoanDBState.Execute();
			} catch (Exception ex) {
				this.Error = ex.Message;
				NL_AddLog(LogType.Error, "Failed on UpdateLoanDBState", Fee, reloadLoanDBState.Error + "\n" + this.Error, ex.ToString(), ex.StackTrace);
			}

		}
	} // class AddPayment
} // ns