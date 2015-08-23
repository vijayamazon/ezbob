namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AddLoanLegals : AStrategy {
        public AddLoanLegals(int customerID, NL_LoanLegals loanLegals) {
            this.customerID = customerID;
            this.loanLegals = loanLegals;
        } //constructor

        public override string Name { get { return "AddLoanLegal"; } }
	    
	    public override void Execute() {

            GetLastOffer g = new GetLastOffer(this.customerID);
            g.Execute();
            var lastOffer = g.Offer;
            
            if (lastOffer.OfferID > 0) {
                this.loanLegals.OfferID = lastOffer.OfferID;
            } else {
                Log.Alert("Last offer not found");
	            Error = "Last offer not found";
				return;
            }

		    try {

			    this.loanLegals.OfferID = lastOffer.OfferID;
			    LoanLegalsID = DB.ExecuteScalar<long>("NL_LoanLegalsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanLegals>("Tbl", this.loanLegals));

			    // ReSharper disable once CatchAllClause
		    } catch (Exception ex) {
				Log.Alert("Failed to add NL_LoanLegals. err: {0}", ex);
			    Error = "Failed to add NL_LoanLegals. Err: " + ex.Message;
		    }
          
        }//Execute

        public long LoanLegalsID { get; set; }
		public string Error { get; set; }

        private readonly int customerID;
        private readonly NL_LoanLegals loanLegals;
    }//class AddLoan
}//ns
