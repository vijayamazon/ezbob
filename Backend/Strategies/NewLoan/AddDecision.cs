namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddDecision : AStrategy {
        public AddDecision(NL_Decisions decision, long? oldCashRequestID) {
            this.decision = decision;
            this.oldCashRequestID = oldCashRequestID;
        }//constructor

        public override string Name { get { return "AddDecision"; } }

        public override void Execute() {
            
            if (this.oldCashRequestID.HasValue) {
                this.decision.CashRequestID =
                    DB.ExecuteScalar<int>("NL_CashRequestGetByOldID",
                        CommandSpecies.StoredProcedure,
                        new QueryParameter("OldCashRequestID", this.oldCashRequestID));

                if (this.decision.CashRequestID == 0) {
                    Log.Error("CashRequestID is 0 for and oldCashRequest {0}", this.oldCashRequestID);
                    return;
                }
            }

            DecisionID = DB.ExecuteScalar<int>("NL_DecisionsSave",
                CommandSpecies.StoredProcedure,
                DB.CreateTableParameter<NL_Decisions>("Tbl", this.decision));
        }//Execute

        public int DecisionID { get; set; }
        private readonly NL_Decisions decision;
        private long? oldCashRequestID;
    }//class AddDecision
}//ns
