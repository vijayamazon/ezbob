namespace Ezbob.Backend.Strategies.LegalDocs.CMS {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;
    public class GetLatestLegalDocs : AStrategy {
        public override string Name {
            get { return "GetLatestLegalDocs"; }
        } // Name
        public override void Execute() {
            LoanAgreementTemplates = DB.Fill<LoanAgreementTemplate>("I_GetLatestLegalDocs", CommandSpecies.StoredProcedure);
        } // Execute
        public List<LoanAgreementTemplate> LoanAgreementTemplates { get; set; }
    } // class GetLatestLegalDocs
} // namespace
