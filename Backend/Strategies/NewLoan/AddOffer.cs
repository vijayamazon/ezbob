namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddOffer : AStrategy {
        public AddOffer(NL_Offers offer) {
            this.offer = offer;
        }//constructor

        public override string Name { get { return "AddOffer"; } }

        public override void Execute() {
            OfferID = DB.ExecuteScalar<int>("NL_OffersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Offers>("Tbl", this.offer)); 
        }//Execute

        public int OfferID { get; set; }
        private readonly NL_Offers offer;
    }//class AddOffer
}//ns
