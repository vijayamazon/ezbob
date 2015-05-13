namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddLoanOptions : AStrategy {
        public AddLoanOptions(NL_LoanOptions loanOptions) {
            this.loanOptions = loanOptions;
        }//constructor

        public override string Name { get { return "AddLoanOptions"; } }

        public override void Execute() {
            LoanOptionsID = DB.ExecuteScalar<int>("NL_LoanOptionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanOptions>("Tbl", this.loanOptions)); 
        }//Execute

        public int LoanOptionsID { get; set; }

        private readonly NL_LoanOptions loanOptions;
    }//class AddLoan
}//ns
