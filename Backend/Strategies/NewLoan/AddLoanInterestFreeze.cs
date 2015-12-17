namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	public class AddLoanInterestFreeze : AStrategy {

		public AddLoanInterestFreeze(NL_LoanInterestFreeze loanInterestFreeze) {
			//oldLoanId = oldLoanid;
			this.loanInterestFreeze = loanInterestFreeze;
			this.strategyArgs = new object[] { oldLoanId, this.loanInterestFreeze };
		}//constructor

		public override string Name { get { return "AddLoanInterestFreeze"; } }
		public string Error { get; set; }
		private int oldLoanId { get; set; }
		public long LoanInterestFreezeID { get; set; }
		private readonly NL_LoanInterestFreeze loanInterestFreeze;
		public long nlLoanInterestFreezeID { get; private set; }

		private readonly object[] strategyArgs;

		public override void Execute() {

			if (!Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value))
				return;

			NL_AddLog(LogType.Info, "Strategy Start", this.loanInterestFreeze, null, Error, null);

			if (oldLoanId == 0) {
				Error = NL_ExceptionRequiredDataNotFound.OldLoan;
				NL_AddLog(LogType.DataExsistense, "End", this.strategyArgs, null, Error, null);
				return;
			}

			try {

				this.loanInterestFreeze.LoanID = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", oldLoanId));

				if (this.loanInterestFreeze.LoanID <= 0) {
					NL_ExceptionLoanForOldIDNotFound er = new NL_ExceptionLoanForOldIDNotFound(oldLoanId);
					Error = er.Message;
					NL_AddLog(LogType.DataExsistense, "End", this.strategyArgs, null, Error, null);
					return;
				}

				nlLoanInterestFreezeID = DB.ExecuteScalar<long>("NL_LoanInterestFreezeSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanInterestFreeze>("Tbl", this.loanInterestFreeze));

				NL_AddLog(LogType.Info, "Strategy End", this.loanInterestFreeze, nlLoanInterestFreezeID, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = string.Format("Failed to save NL_InterestFreeze, oldLoanID: {0}, LoanID: {1}", oldLoanId, this.loanInterestFreeze.LoanID);

				Log.Alert("Failed to save NL_InterestFreeze, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanInterestFreeze.LoanID, ex);

				NL_AddLog(LogType.Error, "Strategy Faild", this.loanInterestFreeze, Error, ex.ToString(), ex.StackTrace);

			}
		}//Execute



	}//class AddInterestFreeze
}//ns
