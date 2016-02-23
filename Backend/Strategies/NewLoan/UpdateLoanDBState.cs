namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// triggered on payment adding/cancellation, rollover, re-scheduling.
	/// Save recalculated loan state differenced to DB
	/// </summary>
	public class UpdateLoanDBState : AStrategy {

		public UpdateLoanDBState(int customerID, long loanID, int userID) {
			CustomerID = customerID;
			LoanID = loanID;
			UserID = userID;
			this.strategyArgs = new object[] { customerID, loanID, userID };
		} // ctor

		public override string Name { get { return "UpdateLoanDBState"; } }

		public string Error { get; private set; }
		public int CustomerID { get; private set; }
		public int UserID { get; private set; }
		public long LoanID { get; private set; }

		private object[] strategyArgs;

		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		public override void Execute() {

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, null, Error, null);

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
				NL_AddLog(LogType.Info, "End - no diff btwn DB state and recalculated state", stateBefore, stateAfter, Error, null);
				return;
			}

			NL_AddLog(LogType.Info, "recalculated loan state", stateBefore, stateAfter, Error, null);

			try {
				bool loanClose = !stateAfter["LoanStatusID"].Equals(stateBefore["LoanStatusID"]);
				SaveLoanStateToDB saveLoan = new SaveLoanStateToDB(RecalculatedModel, loanClose);
				saveLoan.Execute();

			} catch (Exception ex) {
				Error = ex.Message;
				Log.Error("Failed to save updated loan DB dbState. err: {0}", Error);

				NL_AddLog(LogType.Error, "Failed", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
			}
		}
	}

	public class BigintList {
		public long Item { get; set; }
	}
}