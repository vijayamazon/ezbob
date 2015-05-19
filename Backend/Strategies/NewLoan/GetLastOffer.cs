namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class GetLastOffer : AStrategy {
        public GetLastOffer(int customerID) {
            this.customerID = customerID;
        }//constructor

        public override string Name { get { return "GetLastOffer"; } }

        public override void Execute() {
            var sr = DB.GetFirst("NL_OffersGetLast", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", this.customerID));
            Offer = sr.Fill<NL_Offers>();
            Offer.OfferID = sr["OfferID"];
        }//Execute

        public NL_Offers Offer { get; private set; }
        private readonly int customerID;
    }//class GetLastOffer
}//ns
