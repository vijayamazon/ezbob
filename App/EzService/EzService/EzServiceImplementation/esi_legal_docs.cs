namespace EzService.EzServiceImplementation {
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Backend.Strategies.LegalDocs;
    using Ezbob.Backend.Strategies.LegalDocs.CMS;
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

        public LegalDocActionResult AddLegalDoc(int customerID, int userID, LoanAgreementTemplate loanAgreementTemplate) {
            AddLegalDoc strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID, loanAgreementTemplate);
            return new LegalDocActionResult() {
                MetaData = metadata,
                LoanAgreementTemplate = strategy.LoanAgreementTemplate
            };
        }

        public BoolActionResult SaveLegalDoc(int customerID, int userID, LoanAgreementTemplate loanAgreementTemplate) {
            SaveLegalDoc strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID, loanAgreementTemplate);
            return new BoolActionResult() {
                MetaData = metadata,
                Value = strategy.Result
            };
        }

        public IntActionResult ApproveLegalDoc(int customerID, int userID, int loanAgreementTemplateId) {
            ApproveLegalDoc strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID, loanAgreementTemplateId);
            return new IntActionResult() {
                MetaData = metadata,
                Value = strategy.LoanAgreementTemplateId
            };
        }

        public BoolActionResult DeleteLegalDoc(int customerID, int userID, int loanAgreementTemplateId) {
            DeleteLegalDocById strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID, loanAgreementTemplateId);
            return new BoolActionResult() {
                MetaData = metadata,
                Value = strategy.Result
            };
        }
        public LegalDocsActionResult GetLatestLegalDocs(int customerID, int userID) {
            GetLatestLegalDocs strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID);
            return new LegalDocsActionResult() {
                MetaData = metadata,
                LoanAgreementTemplates = strategy.LoanAgreementTemplates
            };
        }
        public LegalDocsActionResult GetLegalDocsPendingApproval(int customerID, int userID) {
            GetLegalDocsPendingApproval strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID);
            return new LegalDocsActionResult() {
                MetaData = metadata,
                LoanAgreementTemplates = strategy.LoanAgreementTemplates
            };
        }

        public BoolActionResult ReviewLegalDoc(int customerID, int userID, int loanAgreementTemplateId) {
            ReviewLegalDoc strategy;
            var metadata = ExecuteSync(out strategy, customerID, userID, loanAgreementTemplateId);
            return new BoolActionResult() {
                MetaData = metadata,
                Value = strategy.Result
            };
        }
    }
}