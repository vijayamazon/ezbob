namespace Ezbob.Backend.Strategies.LegalDocs {
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;

    public class GetLegalDocById : AStrategy {

        public GetLegalDocById(int loanAgreementTemplateId) {
            LoanAgreementTemplateId = loanAgreementTemplateId;
        } // constructor

		public override string Name {
            get { return "GetLegalDocById"; }
		} // Name

		public override void Execute() {
            LoanAgreementTemplate = DB.FillFirst<LoanAgreementTemplate>("I_GetLegalDocById", CommandSpecies.StoredProcedure,
                  new QueryParameter("@LoanAgreementTemplateId", LoanAgreementTemplateId)
                  ); 
		} // Execute

        int LoanAgreementTemplateId { get; set; }
        public LoanAgreementTemplate LoanAgreementTemplate { get; set; }
    } // class Alibaba
} // namespace
