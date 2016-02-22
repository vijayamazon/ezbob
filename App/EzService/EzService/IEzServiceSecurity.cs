namespace EzService {
    using System.ServiceModel;
    using EzService.ActionResults;

    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IEzServiceSecurity {
        [OperationContract]
        SecurityUserActionResult GetSecurityUser(int? userID, int? customerID, string userName, int? originId);
    } // interface IEzServiceSecurity
} // namespace  
