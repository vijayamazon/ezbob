namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddLoanState : AStrategy {
        public AddLoanState(NL_LoanStates loanState) {
            this.loanState = loanState;
        }//constructor

        public override string Name { get { return "AddLoanState"; } }

        public override void Execute() {
            StateID = DB.ExecuteScalar<int>("NL_LoanStatesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanStates>("Tbl", this.loanState)); 
        }//Execute


        public int StateID { get; set; }

        private readonly NL_LoanStates loanState;
    }//class AddLoanState
}//ns
