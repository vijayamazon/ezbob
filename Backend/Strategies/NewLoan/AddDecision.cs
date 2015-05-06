namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddDecision : AStrategy {
        public AddDecision(NL_Decisions decision) {
            this.decision = decision;
        }//constructor

        public override string Name { get { return "AddDecision"; } }

        public override void Execute() {
            DecisionID = DB.ExecuteScalar<int>("NL_DecisionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Decisions>("Tbl", this.decision)); 
        }//Execute

        public int DecisionID { get; set; }
        private readonly NL_Decisions decision;
    }//class AddDecision
}//ns
