namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddOffer : AStrategy {
        public AddOffer(NL_Offers offer, List<NL_OfferFees> fees = null ) {
            this.offer = offer;
	        this.fees = fees;
        }//constructor

        public override string Name { get { return "AddOffer"; } }

        public override void Execute() {

            if (this.offer.DecisionID == 0) {
                Log.Error("DecisionID is 0");
                return;
            }

			ConnectionWrapper pconn = DB.GetPersistent();
	        try {
				pconn.BeginTransaction();

				OfferID = DB.ExecuteScalar<int>(pconn, "NL_OffersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Offers>("Tbl", this.offer));

				if (this.fees != null && this.fees.Count > 0) {
					foreach (NL_OfferFees fee in this.fees) {
						fee.OfferID = OfferID;
						int offerFeeID = DB.ExecuteScalar<int>(pconn,  "NL_OfferFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_OfferFees>("Tbl", fee));
						Log.Debug("OfferFee {0} added", fee);
					}
				}
		        pconn.Commit();

		        // ReSharper disable once CatchAllClause
	        } catch (Exception ex) {
		        Log.Alert("Failed to add NL Offer or OfferFee. Rolling back. {0} ", ex);
		        pconn.Rollback();
	        }
          

        }//Execute

        public int OfferID { get; set; }
        private readonly NL_Offers offer;
		private readonly List<NL_OfferFees> fees;
    }//class AddOffer
}//ns
