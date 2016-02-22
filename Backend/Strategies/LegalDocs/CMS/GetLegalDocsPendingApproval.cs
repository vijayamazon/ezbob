namespace Ezbob.Backend.Strategies.LegalDocs.CMS {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;
    public class GetLegalDocsPendingApproval : AStrategy {
        public override string Name {
            get { return "GetLegalDocsPendingApproval"; }
        } // Name
        public override void Execute() {
            LoanAgreementTemplates = DB.Fill<LoanAgreementTemplate>("I_GetLegalDocsPendingApproval", CommandSpecies.StoredProcedure);
        } // Execute
        public List<LoanAgreementTemplate> LoanAgreementTemplates { get; set; }
    } // class GetLegalDocsPendingApproval
} // namespace
