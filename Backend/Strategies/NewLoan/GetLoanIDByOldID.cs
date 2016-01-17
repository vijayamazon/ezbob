namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	public class GetLoanIDByOldID : AStrategy {

		public GetLoanIDByOldID(int oldID) {
			this.oldLoanId = oldID;
		} // constructor

		public override string Name { get { return "GetLoanIDByOldID"; } }
		public string Error { get; private set; }
		public long LoanID { get; private set; }
		private readonly int oldLoanId;

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.oldLoanId, null, null, null);

			if (this.oldLoanId == 0) {
				Error = NL_ExceptionRequiredDataNotFound.OldLoan;
				Log.Debug(Error);
				NL_AddLog(LogType.DataExsistense, "Inpit invalid", this.oldLoanId, null, Error, null);
				return;
			}

			try {

				LoanID = DB.ExecuteScalar<long>("NL_LoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("OldLoanID", this.oldLoanId));

				if (LoanID <= 0) {
					NL_ExceptionLoanForOldIDNotFound er = new NL_ExceptionLoanForOldIDNotFound(this.oldLoanId);
					Error = er.Message;
					NL_AddLog(LogType.DataExsistense, "End", this.oldLoanId, null, Error, null);
					return;
				}

				NL_AddLog(LogType.Info, "Strategy End", this.oldLoanId, LoanID, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				Log.Info(ex);
				NL_AddLog(LogType.Error, "Strategy Failed", this.oldLoanId, Error, ex.ToString(), ex.StackTrace);
			}
		} // Execute

	} // class GetLoanIDByOldID
} // ns
