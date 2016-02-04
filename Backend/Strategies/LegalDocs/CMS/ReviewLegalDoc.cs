namespace Ezbob.Backend.Strategies.LegalDocs.CMS {
    using System;
    using Ezbob.Database;
    public class ReviewLegalDoc : AStrategy {
        public ReviewLegalDoc(int loanAgreementTemplateId) {
            LoanAgreementTemplateId = loanAgreementTemplateId;
        }
        public override string Name {
            get { return "ReviewLegalDoc"; }
        } // Name
        public override void Execute() {
            try {
                DB.ExecuteNonQuery("I_ReviewLegalDoc", CommandSpecies.StoredProcedure, new QueryParameter("LoanAgreementTemplateId", LoanAgreementTemplateId));
                Result = true;
            }
            catch (Exception) {
                Result = false;
            }
        } // Execute
        public int LoanAgreementTemplateId { get; set; }
        public bool Result { get; set; }
    } // class ReviewLegalDoc
} // namespace
