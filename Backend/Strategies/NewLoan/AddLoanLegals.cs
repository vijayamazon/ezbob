namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies.Exceptions;
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
                Log.Warn("Last offer not found");
            }


            this.loanLegals.OfferID = lastOffer.OfferID;
            LoanLegalsID = DB.ExecuteScalar<int>("NL_LoanLegalSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanLegals>("Tbl", this.loanLegals)); 
        }//Execute

        public int LoanLegalsID { get; set; }

        private readonly int customerID;
        private readonly NL_LoanLegals loanLegals;
    }//class AddLoan
}//ns
