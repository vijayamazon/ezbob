namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using System;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	/// <summary>
	/// Add Collection Log
	/// </summary>
	public class AddCollectionLog : AStrategy {
		public AddCollectionLog(CollectionLog collectionLog) {
			CollectionLog = collectionLog;
		}

		public override string Name { get { return "AddCollectionLog"; } }
		public CollectionLog CollectionLog { get; set; }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", CollectionLog, null, null, null);

			if (CollectionLog.LoanHistoryID == 0) {
				NL_AddLog(LogType.DataExsistense, "Strategy failed", CollectionLog, null, NL_ExceptionRequiredDataNotFound.LastHistory, null);
				return;
			}

			try {
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

				NL_AddLog(LogType.Info, "Strategy End", CollectionLog, CollectionLog.CollectionLogID, null, null);

			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy failed", CollectionLog, null, ex.ToString(), ex.StackTrace);
			}
		}
	}
}