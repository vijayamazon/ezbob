namespace Ezbob.Backend.Strategies.LegalDocs.CMS {
    using Ezbob.Database;
    public class ApproveLegalDoc : AStrategy {
        public ApproveLegalDoc(int loanAgreementTemplateId) {
            LoanAgreementTemplateId = loanAgreementTemplateId;
        }
		public override string Name {
            get { return "ApproveLegalDoc"; }
		} // Name
		public override void Execute() {
            DB.ExecuteNonQuery("I_ApproveLegalDoc", CommandSpecies.StoredProcedure, new QueryParameter("LoanAgreementTemplateId", LoanAgreementTemplateId)); 
		} // Execute
        public int LoanAgreementTemplateId { get; set; }
    } // class ApproveLegalDoc
} // namespace
