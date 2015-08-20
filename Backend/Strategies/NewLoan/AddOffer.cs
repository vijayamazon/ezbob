namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;
	using NHibernate.Linq;
	using NHibernate.Util;

	public class AddOffer : AStrategy {
		public AddOffer(NL_Offers offer, IEnumerable<NL_OfferFees> fees = null) {
            this.offer = offer;
	        this.fees = fees;
        }//constructor

        public override string Name { get { return "AddOffer"; } }

        public override void Execute() {

            if (this.offer.DecisionID == 0) {
                Log.Error("DecisionID is 0");
                return;
            }

	        Log.Debug("{0}", this.offer);

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				OfferID = DB.ExecuteScalar<long>(pconn, "NL_OffersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Offers>("Tbl", this.offer));

				Log.Debug("NL_Offer saved: {0}", OfferID);

				if (this.fees.Any()) {
					this.fees.ForEach(f => f.OfferID = OfferID);
					this.fees.ForEach(f=>Log.Debug("{0}", f));
					DB.ExecuteNonQuery(pconn, "NL_OfferFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_OfferFees>("Tbl", this.fees));
				}

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Alert("Failed to add NL Offer or OfferFee. Rolling back. {0} ", ex);
				//pconn.Rollback();
			}
		
        }//Execute

        public long OfferID { get; set; }
        private readonly NL_Offers offer;
		private readonly IEnumerable<NL_OfferFees> fees;
    }//class AddOffer
}//ns