namespace Ezbob.Backend.Strategies.LegalDocs.CMS {
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;
    public class SyncLegalDocsEnviorments : AStrategy {
        public override string Name {
            get { return "SyncLegalDocsEnviorments"; }
        } // Name
        public override void Execute() {
            try {
                LoanAgreementTemplate = DB.Fill<LoanAgreementTemplate>("I_SyncLegalDocsEnviorments", CommandSpecies.StoredProcedure);
                Result = true;
            }
            catch (Exception) {
                Result = false;
            }
        } // Execute
        public List<LoanAgreementTemplate> LoanAgreementTemplate { get; set; }
        public bool Result { get; set; }
    } // class SyncLegalDocsEnviorments
} // namespace
