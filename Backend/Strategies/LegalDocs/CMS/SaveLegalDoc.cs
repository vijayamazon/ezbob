namespace Ezbob.Backend.Strategies.LegalDocs.CMS {
    using System;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;
    public class SaveLegalDoc : AStrategy {
        public SaveLegalDoc(LoanAgreementTemplate loanAgreementTemplate) {
            LoanAgreementTemplate = loanAgreementTemplate;
        }
        public override string Name {
            get { return "SaveLegalDoc"; }
        } // Name
        public override void Execute() {
            try {
                DB.ExecuteNonQuery("I_SaveLegalDoc", CommandSpecies.StoredProcedure,
                new QueryParameter("LoanAgreementTemplateId", LoanAgreementTemplate.Id),
                new QueryParameter("Template", LoanAgreementTemplate.Template)
                );
                Result = true;
            }
            catch (Exception) {
                Result = false;
            }

        } // Execute
        public LoanAgreementTemplate LoanAgreementTemplate { get; set; }
        public bool Result { get; set; }
    } // class SaveLegalDoc
} // namespace
