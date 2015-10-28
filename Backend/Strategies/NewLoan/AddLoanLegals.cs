﻿namespace Ezbob.Backend.Strategies.NewLoan {
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AddLoanLegals : AStrategy {
        public AddLoanLegals(int customerID, NL_LoanLegals loanLegals) {
            this.customerID = customerID;
            this.loanLegals = loanLegals;
        } //constructor

        public override string Name { get { return "AddLoanLegal"; } }

	    /// <exception cref="NL_ExceptionOfferNotValid">Condition. </exception>
	    public override void Execute() {

            GetLastOffer g = new GetLastOffer(this.customerID);
            g.Execute();
            var lastOffer = g.Offer;
            
            if (lastOffer.OfferID > 0) {
                this.loanLegals.OfferID = lastOffer.OfferID;
            } else {
                Log.Alert("Last offer not found");

	            throw new NL_ExceptionOfferNotValid();
            }


            this.loanLegals.OfferID = lastOffer.OfferID;
            LoanLegalsID = DB.ExecuteScalar<int>("NL_LoanLegalsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanLegals>("Tbl", this.loanLegals)); 
        }//Execute

        public int LoanLegalsID { get; set; }

        private readonly int customerID;
        private readonly NL_LoanLegals loanLegals;
    }//class AddLoan
}//ns
