namespace Ezbob.Backend.Strategies.NewLoan.Collection
{
    using System;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Database;

    /// <summary>
    /// Add Collection Log
    /// </summary>
    public class AddCollectionLog : AStrategy
    {
        public AddCollectionLog(CollectionLog collectionLog) {
            CollectionLog = collectionLog;
        }

        public override string Name { get { return "Add Collection Log"; } }
        public CollectionLog CollectionLog { get; set; }

        public override void Execute(){
            try{
                NL_AddLog(LogType.Info, "Strategy Start", new object[] { CollectionLog.CollectionLogID, CollectionLog.CustomerID, CollectionLog.LoanID, CollectionLog.Type, CollectionLog.Method, CollectionLog.LoanHistoryID, CollectionLog.Comments }, null, null, null);
                Log.Info("Adding collection log to customer {0} loan {1} type {2} method {3}", CollectionLog.CustomerID, CollectionLog.LoanID, CollectionLog.Type, CollectionLog.TimeStamp);
                CollectionLog.CollectionLogID = DB.ExecuteScalar<int>("AddCollectionLog",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("CustomerID", CollectionLog.CustomerID),
                    new QueryParameter("LoanID", CollectionLog.LoanID),
                    new QueryParameter("Type", CollectionLog.Type),
                    new QueryParameter("Method", CollectionLog.Method),
                    new QueryParameter("LoanHistoryID", CollectionLog.LoanHistoryID),
                    new QueryParameter("Comments", CollectionLog.Comments),
                    new QueryParameter("Now", CollectionLog.TimeStamp));
                NL_AddLog(LogType.Info, "Strategy End", null, new object[] { CollectionLog.CollectionLogID, CollectionLog.CustomerID, CollectionLog.LoanID, CollectionLog.Type, CollectionLog.Method, CollectionLog.LoanHistoryID, CollectionLog.Comments }, null, null);
            }
            catch (Exception ex){
                NL_AddLog(LogType.Error, "Strategy Faild", null, null, ex.ToString(), ex.StackTrace);
            }

        }//Execute

    }// class CollectionLog
} // namespace


