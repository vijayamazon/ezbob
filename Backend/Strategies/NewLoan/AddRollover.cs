namespace Ezbob.Backend.Strategies.NewLoan {
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AddRollover : AStrategy {
        public AddRollover(NL_LoanRollovers rollover) {
            this.rollover = rollover;
        }//constructor

        public override string Name { get { return "AddRollover"; } }

        public override void Execute() {
            RolloverID = DB.ExecuteScalar<int>("NL_LoanRolloversSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanRollovers>("Tbl", this.rollover)); 
        }//Execute


        public int RolloverID { get; set; }

        private readonly NL_LoanRollovers rollover;
    }//class AddRollover
}//ns
