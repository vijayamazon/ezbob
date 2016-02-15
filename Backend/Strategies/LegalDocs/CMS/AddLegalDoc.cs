namespace Ezbob.Backend.Strategies.LegalDocs.CMS {
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;
    public class AddLegalDoc : AStrategy {
        public AddLegalDoc(LoanAgreementTemplate loanAgreementTemplate) {
            LoanAgreementTemplate = loanAgreementTemplate;
        }
		public override string Name {
            get { return "AddLegalDoc"; }
		} // Name
		public override void Execute() {
            LoanAgreementTemplateId = DB.ExecuteScalar<int>("I_AddLegalDoc", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", LoanAgreementTemplate));
            Result = LoanAgreementTemplate = DB.FillFirst<LoanAgreementTemplate>("I_GetLegalDocById", CommandSpecies.StoredProcedure,
                  new QueryParameter("@LoanAgreementTemplateId", LoanAgreementTemplateId)); 
		} // Execute
        public LoanAgreementTemplate LoanAgreementTemplate { get; set; }
        public int LoanAgreementTemplateId { get; set; }
        public LoanAgreementTemplate Result { get; set; }
    } // class AddLegalDocDraft
} // namespace
