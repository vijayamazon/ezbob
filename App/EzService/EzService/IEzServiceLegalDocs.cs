namespace EzService {
    using System.ServiceModel;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using EzService.ActionResults.Investor;

    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IEzServiceLegalDocs {
        [OperationContract]
        LegalDocsActionResult GetLegalDocs(int customerID, int userID, int originID, bool isRegulated, int productSubTypeID);
        [OperationContract]
        LegalDocActionResult GetLegalDocById(int customerID, int userID, int loanAgreementTemplateId);
        [OperationContract]
        BoolActionResult ManualLegalDocsSyncTemplatesFiles(string agreementsPath);
        [OperationContract]
        LegalDocActionResult AddLegalDoc(int customerID, int userID, LoanAgreementTemplate loanAgreementTemplate);
        [OperationContract]
        IntActionResult ApproveLegalDoc(int customerID, int userID, int loanAgreementTemplateId);
        [OperationContract]
        LegalDocsActionResult GetLatestLegalDocs(int customerID, int userID);
        [OperationContract]
        LegalDocsActionResult GetLegalDocsPendingApproval(int customerID, int userID);
        [OperationContract]
        BoolActionResult ReviewLegalDoc(int customerID, int userID, int loanAgreementTemplateId);
        [OperationContract]
        BoolActionResult DeleteLegalDoc(int customerID, int userID, int loanAgreementTemplateId);
        [OperationContract]
        BoolActionResult SaveLegalDoc(int customerID, int userID, LoanAgreementTemplate loanAgreementTemplate);
    } // interface IEzServiceLegalDocs
} // namespace  
