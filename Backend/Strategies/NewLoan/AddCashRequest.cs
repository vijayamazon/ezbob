namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddCashRequest : AStrategy {
        public AddCashRequest(NL_CashRequests cashRequest) {
            this.cashRequest = cashRequest;
        }//constructor

        public override string Name { get { return "AddCashRequest"; } }

        public override void Execute() {
            CashRequestID = DB.ExecuteScalar<int>("NL_CashRequestsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_CashRequests>("Tbl", this.cashRequest)); 
        }//Execute


        public int CashRequestID { get; set; }

        private readonly NL_CashRequests cashRequest;
    }//class AddCashRequest
}//ns
