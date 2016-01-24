namespace EzService {
    using System.ServiceModel;
    using EzService.ActionResults.Investor;

    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IEzServiceLegalDocs {
        [OperationContract]
        LegalDocsActionResult GetLegalDocs(int customerID, int userID, int originID, bool isRegulated, int productSubTypeID);
        [OperationContract]
        LegalDocActionResult GetLegalDocById(int customerID, int userID, int loanAgreementTemplateId);
        [OperationContract]
        BoolActionResult ManualLegalDocsSyncTemplatesFiles(int customerID, int userID);
    } // interface IEzServiceLegalDocs
} // namespace  
