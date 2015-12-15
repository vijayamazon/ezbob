namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	public class GetLoanIDByOldID : AStrategy {

		public GetLoanIDByOldID(int oldID) {
			this.OldLoanId = oldID;
		} // constructor

		public override string Name { get { return "GetLoanIDByOldID"; } }
		public string Error { get; private set; }
		public long LoanID { get; private set; }
		private readonly long OldLoanId;

		public override void Execute() {

			if (!Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value))
				return;

			NL_AddLog(LogType.Info, "Strategy Start", this.OldLoanId, null, null, null);

			if (this.OldLoanId == 0) {
				Error = NL_ExceptionRequiredDataNotFound.OldLoan;
				NL_AddLog(LogType.Info, "Strategy Failed", this.OldLoanId, null, Error, null);
				return;
			}

			try {
				LoanID = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", this.OldLoanId));

				NL_AddLog(LogType.Info, "Strategy End", this.OldLoanId, LoanID, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				NL_AddLog(LogType.Error, "Strategy Failed", this.OldLoanId, Error, ex.ToString(), ex.StackTrace);
			}
		} // Execute

	} // class GetLoanIDByOldID
} // ns
