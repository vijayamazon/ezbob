namespace EzService.EzServiceImplementation {
    using Ezbob.Backend.Strategies.LegalDocs;
    using EzService.ActionResults.Investor;

    partial class EzServiceImplementation : IEzServiceLegalDocs {
        public LegalDocsActionResult GetLegalDocs(int customerID, int userID, int originID, bool isRegulated, int productSubTypeID) {
            GetLegalDocs strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID, originID, isRegulated, productSubTypeID);
            return new LegalDocsActionResult() {
                MetaData = metadata,
                LoanAgreementTemplates = strategy.LoanAgreementTemplate
            };
        }

        public LegalDocActionResult GetLegalDocById(int customerID, int userID, int loanAgreementTemplateId) {
            GetLegalDocById strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID, loanAgreementTemplateId);
            return new LegalDocActionResult() {
                MetaData = metadata,
                LoanAgreementTemplate = strategy.LoanAgreementTemplate
            };
        }

        public BoolActionResult ManualLegalDocsSyncTemplatesFiles(string ageementsPath) {
            ManualLegalDocsSyncTemplatesFiles strategy;
			var metadata = ExecuteSync(out strategy, null, null, ageementsPath);
            return new BoolActionResult {
                MetaData = metadata,
                Value = strategy.Result
            };
        }
    }
}