namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	/// <summary>
	/// adding new rollover proposal for loan or updates existing valid rollover?
	/// </summary>
	public class SaveRollover : AStrategy {

		public SaveRollover(NL_LoanRollovers r, long loanID) {
			rollover = r;
			LoanID = loanID;
			this.strategyArgs = new object[] { LoanID, rollover };
		}

		public override string Name { get { return "AddRollover"; } }

		public NL_LoanRollovers rollover { get; private set; }
		public long RolloverID { get; private set; }
		public long LoanID { get; private set; }
		public string Error { get; private set; }

		private readonly object[] strategyArgs;

		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		public override void Execute() {

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			// EZ-4329 
			// check if same rollover exists OK
			// add RolloverFee OK
			// add opportunity for rollover (insert into NL_LoanRollovers)  OK

			NL_AddLog(LogType.Info, "Started", this.strategyArgs, Error, null, null);

			if (LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionLoanNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionLoanNotFound(Error);
			}

			try {

				//List<NL_LoanRollovers> loanRollovers = DB.Fill<NL_LoanRollovers>("NL_RolloversGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", LoanID));
				//var rExists = loanRollovers.FirstOrDefault(r => r.CreationTime.Date.Equals(rollover.CreationTime.Date) && r.ExpirationTime.Date.Equals(rollover.ExpirationTime.Date));

				//if (rExists != null) {
				//	Error = string.Format("Rollover opportunity for loan {0}, added at {1}, expired at {2}, already registered in the system.", LoanID, rollover.CreationTime, rollover.ExpirationTime);
				//	Log.Info(Error);
				//	NL_AddLog(LogType.Info, "Started", this.strategyArgs, Error, null, null);
				//	return;
				//}

				if (rollover.LoanRolloverID == 0) {
					// inser rollover 
					rollover.LoanRolloverID = DB.ExecuteScalar<long>("NL_LoanRolloversSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanRollovers>("Tbl", rollover));
				} else {
					// update rollover
					DB.ExecuteNonQuery("NL_LoanRolloverUpdate", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanRollovers>("Tbl", rollover), new QueryParameter("RolloverID", rollover.LoanRolloverID));
				}
	
				RolloverID = rollover.LoanRolloverID;

				NL_AddLog(LogType.Info, "End", this.strategyArgs, rollover, Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				//pconn.Rollback();

				Error = ex.Message;
				Log.Error("Failed to save rollover {0}. err: {1}", rollover, Error);

				NL_AddLog(LogType.Error, "Strategy Faild - Rollback", rollover, Error, ex.ToString(), ex.StackTrace);
			}

		} // Execute
	}
}