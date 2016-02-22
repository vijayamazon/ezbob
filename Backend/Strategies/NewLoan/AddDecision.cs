namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AddDecision : AStrategy {
		public AddDecision(NL_Decisions decision, long? oldCashRequestID, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons) {
			this.decision = decision;
			this.oldCashRequestID = oldCashRequestID;
			this.decisionRejectReasons = (List<NL_DecisionRejectReasons>)decisionRejectReasons;

			this.strategyArgs = new object[] { decision, oldCashRequestID, decisionRejectReasons };
		}//constructor

		public override string Name { get { return "AddDecision"; } }

		public long DecisionID { get; private set; }
		public string Error { get; private set; }

		private readonly NL_Decisions decision;
		private long? oldCashRequestID;
		private readonly List<NL_DecisionRejectReasons> decisionRejectReasons;

		private readonly object[] strategyArgs;

		public override void Execute() {

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, this.decision, null, null);

			Log.Debug("ADDIND decision: {0}", this.decision);

			try {
				if (this.oldCashRequestID.HasValue) {

					this.decision.CashRequestID = DB.ExecuteScalar<long>("NL_CashRequestGetByOldID", CommandSpecies.StoredProcedure, new QueryParameter("OldCashRequestID", this.oldCashRequestID));

					Log.Info("cashRequestID: {0}", this.decision.CashRequestID);

					if (this.decision.CashRequestID == 0) {
						Log.Info("CashRequestID is 0 for and oldCashRequest {0}", this.oldCashRequestID);
						Error = string.Format("CashRequestID is 0 for and oldCashRequest {0}", this.oldCashRequestID);
						NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, this.decision, Error, null);
						return;
					}
				}

				Log.Debug(this.decision);

				DecisionID = DB.ExecuteScalar<long>("NL_DecisionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", this.decision));

				if ((this.decisionRejectReasons !=null) && (this.decisionRejectReasons.Count > 0)) {
					this.decisionRejectReasons.ForEach( rr=>rr.DecisionID = DecisionID);
					DB.ExecuteNonQuery("NL_DecisionRejectReasonsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_DecisionRejectReasons>("Tbl", this.decisionRejectReasons));
				}

				NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, new object[] { this.decision, DecisionID }, Error, null);

			} catch (DbException dbException) {
				
				if(dbException.Message.Contains("Violation of UNIQUE KEY constraint"))
					NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, new object[] { this.decision, DecisionID }, Error, null);
				else 
					LogAndExit(dbException);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				LogAndExit(ex);
			}
		}

		private void LogAndExit(Exception ex) {
			Error = ex.Message;
			Log.Alert("Failed to save NL_Decision for oldCashrequestID {0}, err {1}", this.oldCashRequestID, ex);
			Error = string.Format("Failed to save NL_Decision for oldCashrequestID {0}, err {1}", this.oldCashRequestID, ex.Message);
			NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, this.decision, Error, ex.StackTrace);
		}

	}//class AddDecision
}//ns