namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AddLoanState : AStrategy {
		public AddLoanState(NL_LoanStates loanState) {
			this.loanState = loanState;
		}//constructor

		public override string Name { get { return "AddLoanState"; } }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			try {
				NL_AddLog(LogType.Info, "Strategy Start", this.loanState, null, null, null);
				StateID = DB.ExecuteScalar<int>("NL_LoanStatesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanStates>("Tbl", this.loanState));
				NL_AddLog(LogType.Info, "Strategy End", this.loanState, StateID, null, null);
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy Faild", this.loanState, null, ex.ToString(), ex.StackTrace);
			}
		}//Execute


		public int StateID { get; set; }

		private readonly NL_LoanStates loanState;
	}//class AddLoanState
}//ns
