namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddFee : AStrategy {
        public AddFee(NL_LoanFees fee) {
            this.fee = fee;
        }//constructor

        public override string Name { get { return "AddFee"; } }

        public override void Execute() {
            FeeID = DB.ExecuteScalar<int>("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", this.fee)); 
        }//Execute

        public int FeeID { get; set; }

        private readonly NL_LoanFees fee;
    }//class AddFee
}//ns
