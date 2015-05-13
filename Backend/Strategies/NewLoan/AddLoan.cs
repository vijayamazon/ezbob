namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddLoan : AStrategy {
        public AddLoan(NL_Loans loan) {
            this.loan = loan;
        }//constructor

        public override string Name { get { return "AddLoan"; } }

        public override void Execute() {
            LoanID = DB.ExecuteScalar<int>("NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Loans>("Tbl", this.loan)); 
        }//Execute


        public int LoanID { get; set; }

        private readonly NL_Loans loan;
    }//class AddLoan
}//ns
