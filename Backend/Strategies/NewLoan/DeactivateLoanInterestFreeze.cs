namespace Ezbob.Backend.Strategies.NewLoan
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class DeactivateLoanInterestFreeze : AStrategy
    {

        public string Error { get; set; }
        private int? oldLoanId { get; set; }
        public int OldLoanInterestFreezeID { get; set; }
        public DateTime? DeactivationDate { get; set; }

        public DeactivateLoanInterestFreeze(int? OldLoanId,
                                            int oldLoanInterestFreezeID,
                                            DateTime deactivationDate)
        {
            this.oldLoanId = OldLoanId;      
            this.OldLoanInterestFreezeID = oldLoanInterestFreezeID;
            this.DeactivationDate = deactivationDate;
        }//constructor

        public override string Name { get { return "DeactivateLoanInterestFreeze"; } }

        public override void Execute()
        {
            if (!Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value))
                return;
            NL_AddLog(LogType.Info, "Strategy Start", this.OldLoanInterestFreezeID, null, null, null);
            try {
                long? newLoanId = null;

                if (oldLoanId != null)
                    newLoanId = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", oldLoanId));

                List<NL_LoanInterestFreeze> FreezeInterestIntervals = null;
                
                if (newLoanId != null) {
                    FreezeInterestIntervals  = DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet",CommandSpecies.StoredProcedure,new QueryParameter("@LoanID", newLoanId));
                }
                long nlInterestFreezeID = -1;
                if (FreezeInterestIntervals != null) {
                    var nlLoanInterestFreeze = FreezeInterestIntervals.FirstOrDefault(x=>x.OldID == this.OldLoanInterestFreezeID);
                    if (nlLoanInterestFreeze != null) {
                        var userID = Context.UserID;
                        if (userID != null) {
                            nlInterestFreezeID = nlLoanInterestFreeze.LoanInterestFreezeID;
                           DB.ExecuteNonQuery("NL_LoanInterestFreezeDeactivate",
                                CommandSpecies.StoredProcedure,
                                new QueryParameter("UserID", (int)userID),
                                new QueryParameter("LoanInterestFreezeID", nlInterestFreezeID),
                                new QueryParameter("DeactivationDate", DeactivationDate));
                        }
                    }
                }

                NL_AddLog(LogType.Info, "Strategy End", this.OldLoanInterestFreezeID, nlInterestFreezeID, null, null);
            }
            catch (Exception ex)
            {
                Log.Alert("Failed to execute SP: DeactivateLoanInterestFreeze, oldLoanID: {0}, OldLoanInterestFreezeID: {1}, ex: {2}", oldLoanId, OldLoanInterestFreezeID, ex);
                Error = string.Format("Failed to execute SP: DeactivateLoanInterestFreeze, oldLoanID: {0}, LoanInterestFreezeID: {1}, ex: {2}", oldLoanId, OldLoanInterestFreezeID, ex.Message);
                NL_AddLog(LogType.Error, "Strategy Faild", this.OldLoanInterestFreezeID,null, ex.ToString(), ex.StackTrace);
            }
        }//Execute

    }//class DeactivateLoanInterestFreeze
}//ns
