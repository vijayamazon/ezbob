namespace Ezbob.Backend.Strategies.NewLoan {
    using System;
    using System.Linq;
    using ConfigManager;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class DeactivateLoanInterestFreeze : AStrategy {

		public DeactivateLoanInterestFreeze(NL_LoanInterestFreeze freezeInterval) {
		    LoanFreezeInterval = freezeInterval;
		}//constructor

		public override string Name { get { return "DeactivateLoanInterestFreeze"; } }

		public string Error { get; set; }
        public NL_LoanInterestFreeze LoanFreezeInterval { get; set; }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

            NL_AddLog(LogType.Info, "Strategy Start", LoanFreezeInterval, null, null, null);

			try {

			    var freezeInterestIntervals = DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", LoanFreezeInterval.LoanID));
				
                long nlInterestFreezeID = -1;

				if (freezeInterestIntervals != null) {
					var nlLoanInterestFreeze = freezeInterestIntervals.FirstOrDefault(x => x.OldID == LoanFreezeInterval.OldID);
					if (nlLoanInterestFreeze != null) {
						var userID = Context.UserID;
						if (userID != null) {
							nlInterestFreezeID = nlLoanInterestFreeze.LoanInterestFreezeID;
							DB.ExecuteNonQuery("NL_LoanInterestFreezeDeactivate",CommandSpecies.StoredProcedure,
                                 new QueryParameter("UserID", LoanFreezeInterval.AssignedByUserID),
								 new QueryParameter("LoanInterestFreezeID", nlInterestFreezeID),
                                 new QueryParameter("DeactivationDate", LoanFreezeInterval.DeactivationDate));
						}
					}
				}

                NL_AddLog(LogType.Info, "Strategy End", LoanFreezeInterval, nlInterestFreezeID, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
                Log.Alert("Failed to execute SP: DeactivateLoanInterestFreeze, newLoanID: {0}, LoanInterestFreezeID: {1}, ex: {2}", LoanFreezeInterval.LoanID, LoanFreezeInterval.OldID, ex);
                Error = string.Format("Failed to execute SP: DeactivateLoanInterestFreeze, newLoanID: {0}, LoanInterestFreezeID: {1}, ex: {2}", LoanFreezeInterval.LoanID, LoanFreezeInterval.OldID, ex.Message);
                NL_AddLog(LogType.Error, "Strategy Faild", LoanFreezeInterval.OldID, null, ex.ToString(), ex.StackTrace);
			}
		}//Execute

	}//class DeactivateLoanInterestFreeze
}//ns
