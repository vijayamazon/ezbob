namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	public class AcceptRollover : AStrategy {

		// check rollover opportunity is not expired
		// rolloverFee  - call AddPayment strategy directly from caller that registeres rollover payment
		// then call AcceptRollover
		public AcceptRollover(int customerID, long loanID) {
			CustomerID = customerID;
			LoanID = loanID;
		}

		/* rollover pay:
		1. \ezbob\App\PluginWeb\EzBob.Web\Areas\Customer\Controllers\PaypointController.cs => public ActionResult Pay(decimal amount, string type, int loanId, int rolloverId)
		
		2. \ezbob\App\PluginWeb\EzBob.Web\Areas\Customer\Controllers\PaypointController.cs => public ActionResult Callback
		 (	bool valid,	string trans_id,string code,string auth_code,decimal? amount,string ip,	string test_status,	string hash,string message,	string type,int loanId,	string card_no,	string customer,string expiry) 
		
		3. \ezbob\Integration\PaymentServices\Calculators\LoanPaymentFacade.cs => public PaymentResult MakePayment
		 (string transId,	decimal amount,	string ip,	string type,int loanId,	Customer customer,DateTime? date = null,string description = "payment from customer",string paymentType = null,	string sManualPaymentMethod = null,
		 NL_Payments nlPayment = null)
		 
			line 497
		 */

		public override string Name { get { return "AcceptRollover"; } }
		public int CustomerID { get; private set; }
		public long LoanID { get; private set; }
		public string Error { get; private set; }

		private object[] strategyArgs;

		/// <exception cref="ArgumentNullException"><paramref /> is null. </exception>
		/// <exception cref="FormatException"><paramref /> is not in the correct format. </exception>
		public override void Execute() {

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			this.strategyArgs = new object[] { CustomerID, LoanID };

			if (CustomerID == 0) {
				Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.DataExsistense, "Strategy failed", this.strategyArgs, null, Error, null);
				return;
			}

			if (LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.DataExsistense, "Strategy failed", this.strategyArgs, null, Error, null);
				return;
			}

			// TODO EZ-4330
			/* RolloverPayment = open interest till accept day included + rollover fees assigned
			1. records NL_Payments (+NL_PaypointTransactions) for rollover () - via AddPayment strategy from outside - via customer dashboard before this strategy
			2. record rollover fee for into NL_LoanFees
			3. update existing rollover record: [CustomerActionTime], [IsAccepted] in [dbo].[NL_LoanRollovers] table
			5. make rollover using calculator - add to NL model new history, rearrange schedules, statuses
			 *  - run calc.GetState to get outstanding balance
			 *  - create new history
			 *  - mark non relevant schedules as CancelledOnRollover
			 *  - create schedule for new history
			6. update DB: records new history, new schedule items; update previous schedule with appropriate statuses
			*/

			DateTime nowTime = DateTime.Now;
			bool rolloverAccepted = false;

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, null, Error, null);

			// fetch loan's rollovers and check rollover exists, valid and not accepted yet
			NL_LoanRollovers rollover=DB.Fill<NL_LoanRollovers>("NL_RolloversGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", LoanID))
				.FirstOrDefault(r => r.DeletedByUserID == null && r.DeletionTime == null && r.IsAccepted == false && (r.CreationTime <= nowTime && nowTime <= r.ExpirationTime));

			if (rollover == null) {
				Error = string.Format("Rollover opportunity for loan {0} not found, or expired, or accepted", LoanID);
				Log.Info(Error);
				NL_AddLog(LogType.DataExsistense, "Strategy end", this.strategyArgs, null, Error, null);
				rolloverAccepted = true;
			}

			if (!rolloverAccepted) {
				// set rollover data for future update
				rollover.IsAccepted = true;
				rollover.CustomerActionTime = nowTime;

				ConnectionWrapper pconn = DB.GetPersistent();

				try {

					// insert rollover fee
					NL_LoanFees rolloverFee = new NL_LoanFees() {
						LoanID = LoanID,
						Amount = Decimal.Parse(CurrentValues.Instance.RolloverCharge.Value),
						AssignedByUserID = rollover.CreatedByUserID,
						AssignTime = rollover.CustomerActionTime.Value,
						CreatedTime = rollover.CustomerActionTime.Value, //DateTime.UtcNow,
						LoanFeeTypeID = (int)NLFeeTypes.RolloverFee,
						Notes = string.Format("rolloverID {2} {0:d}-{1:d}", rollover.CreationTime, rollover.ExpirationTime, rollover.LoanRolloverID)
					};

					rollover.LoanFeeID = DB.ExecuteScalar<long>("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", rolloverFee));

					Log.Info("NL rollover fee {0} added", rollover.LoanFeeID);
					NL_AddLog(LogType.Info, "Rollover fee added", this.strategyArgs, rollover.LoanFeeID, Error, null);

					// set newly created history ID for rollover row
					//rollover.LoanHistoryID = loanState.Result.Loan.LastHistory().LoanHistoryID;

					// update rollover
					DB.ExecuteNonQuery("NL_LoanRolloverUpdate", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanRollovers>("Tbl", rollover), new QueryParameter("RolloverID", rollover.LoanRolloverID));

					pconn.Commit();

					//ReSharper disable once CatchAllClause
				} catch (Exception ex) {

					pconn.Rollback();

					Error = ex.Message;
					Log.Error("Failed to 'accept rollover': {0}", Error);
					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);

					return;
				}
			}

			// update "old" schedules, add new history + new schedules + new distributed fees to DB
			UpdateLoanDBState updateState = new UpdateLoanDBState(CustomerID, LoanID, 1);
			try {
				updateState.Execute();
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				Log.Alert(Error);
				NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, null);
			}

			// reassign payments and save
			try {
				updateState.Execute();
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				Log.Alert(Error);
				NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, null);
			}

		}

	} // class AcceptRollover
} // ns