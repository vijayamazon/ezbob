namespace Ezbob.Backend.Strategies.NewLoan {
    using System;
    using ConfigManager;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddLoanInterestFreeze : AStrategy {

        public AddLoanInterestFreeze(NL_LoanInterestFreeze loanInterestFreeze) {
            LoanFreezeInterval = loanInterestFreeze;
        }//constructor

        public override string Name { get { return "AddLoanInterestFreeze"; } }
        public string Error { get; set; }
        public NL_LoanInterestFreeze LoanFreezeInterval { get; set; }

        public override void Execute() {

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

            NL_AddLog(LogType.Info, "Strategy Start", LoanFreezeInterval, null, Error, null);
            try {
                LoanFreezeInterval.LoanInterestFreezeID = DB.ExecuteScalar<long>("NL_LoanInterestFreezeSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanInterestFreeze>("Tbl", LoanFreezeInterval));
                NL_AddLog(LogType.Info, "Strategy End", null, LoanFreezeInterval, null, null);
            }
            catch (Exception ex) {
                Error = string.Format("Failed to save NL_InterestFreeze, LoanID: {0}, LoanInterestFreezeID: {1}", LoanFreezeInterval.LoanID, LoanFreezeInterval.LoanInterestFreezeID);
                Log.Alert("Failed to save NL_InterestFreeze, LoanID: {0}, LoanInterestFreezeID: {1}, ex: {2}", LoanFreezeInterval.LoanID, LoanFreezeInterval.LoanInterestFreezeID, ex);
                NL_AddLog(LogType.Error, "Strategy Faild", LoanFreezeInterval, Error, ex.ToString(), ex.StackTrace);
            }
        }//Execute

    }//class AddInterestFreeze
}//ns
