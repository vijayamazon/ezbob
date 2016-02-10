namespace Ezbob.Backend.Strategies.LegalDocs.CMS {
    using System;
    using Ezbob.Database;

    public class DeleteLegalDocById : AStrategy {
        public DeleteLegalDocById(int loanAgreementTemplateId) {
            LoanAgreementTemplateId = loanAgreementTemplateId;
        }
		public override string Name {
            get { return "AddLegalDoc"; }
		} // Name
		public override void Execute() {

            try {
                DB.ExecuteNonQuery("I_ReviewLegalDoc", CommandSpecies.StoredProcedure, new QueryParameter("LoanAgreementTemplateId", LoanAgreementTemplateId));
                Result = true;
            }
            catch (Exception) {
                Result = false;
            }
            LoanAgreementTemplateId = DB.ExecuteNonQuery("I_DeleteLegalDocById", CommandSpecies.StoredProcedure, new QueryParameter("LoanAgreementTemplateId", LoanAgreementTemplateId));
		} // Execute
        public int LoanAgreementTemplateId { get; set; }
        public bool Result { get; set; }
    } // class AddLegalDocDraft
} // namespace
