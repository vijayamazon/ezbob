namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;
    using NHibernate.Util;

    public class AddDecision : AStrategy {
        public AddDecision(NL_Decisions decision, long? oldCashRequestID, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons) {
            this.decision = decision;
            this.oldCashRequestID = oldCashRequestID;
            this.decisionRejectReasons = decisionRejectReasons;
        }//constructor

        public override string Name { get { return "AddDecision"; } }

        public override void Execute() {

			try {
				if (this.oldCashRequestID.HasValue) {

					this.decision.CashRequestID = DB.ExecuteScalar<int>("NL_CashRequestGetByOldID", CommandSpecies.StoredProcedure, new QueryParameter("OldCashRequestID", this.oldCashRequestID));

					Log.Info("cashRequestID: {0}", this.decision.CashRequestID);

					if (this.decision.CashRequestID == 0) {
						Log.Error("CashRequestID is 0 for and oldCashRequest {0}", this.oldCashRequestID);
						return;
					}
				}

				Log.Debug(this.decision);

				DecisionID = DB.ExecuteScalar<int>("NL_DecisionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Decisions>("Tbl", this.decision));

				if (this.decisionRejectReasons != null && this.decisionRejectReasons.Any()) {
					foreach (var decisionRejectReason in this.decisionRejectReasons) {
						decisionRejectReason.DecisionID = DecisionID;
					}

					DB.ExecuteNonQuery("NL_DecisionRejectReasonsSave",
						CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<NL_DecisionRejectReasons>("Tbl", this.decisionRejectReasons));
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Alert("Failed to save NL_Decision for oldCashrequestID {0}, err {1}", this.oldCashRequestID, ex);
			}

        }//Execute

        public int DecisionID { get; set; }
        private readonly NL_Decisions decision;
        private long? oldCashRequestID;
        private readonly IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons;
    }//class AddDecision
}//ns