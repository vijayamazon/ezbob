namespace Ezbob.Backend.Strategies.NewLoan {
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AddFee : AStrategy {
        public AddFee(NL_LoanFees fee) {
            this.fee = fee;
        }//constructor

        public override string Name { get { return "AddFee"; } }

        public override void Execute() {
			FeeID = DB.ExecuteScalar<long>("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", this.fee)); 
        }//Execute

		public long FeeID { get; set; }

        private readonly NL_LoanFees fee;
    }//class AddFee
}//ns
