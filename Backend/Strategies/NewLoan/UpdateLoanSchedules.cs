namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Database;

	public class UpdateLoanSchedules : AStrategy {

		public UpdateLoanSchedules(long loanScheduleID, QueryParameter[] queryParameteres, int? oldLoanId) {
			LoanScheduleID = loanScheduleID;
			this.oldLoanId = oldLoanId;
			QueryParameteres = queryParameteres;

			this.strategyArgs = new object[] { LoanScheduleID, QueryParameteres, oldLoanId };
		}//constructor

		public int? oldLoanId { get; set; }
		public string Error { get; set; }
		public long LoanScheduleID { get; set; }

		public QueryParameter[] QueryParameteres { get; set; }

		public override string Name { get { return "UpdateLoanSchedules"; } }

		private readonly object[]  strategyArgs;

		public override void Execute() {
			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, null, null, null);
			try {
				DB.ExecuteNonQuery("NL_LoanSchedulesUpdate",
						CommandSpecies.StoredProcedure,
						this.QueryParameteres);
				NL_AddLog(LogType.Info, "Strategy End", null, LoanScheduleID, null, null);
			} catch (Exception ex) {
				Log.Alert("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanScheduleID: {1}, ex: {2}", oldLoanId, LoanScheduleID, ex);
				Error = string.Format("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanScheduleID: {1}, ex: {2}", oldLoanId, LoanScheduleID, ex.Message);
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, ex.ToString(), ex.StackTrace);
			}
		}//Execute


	}//class AddLoanOptions
}//ns
