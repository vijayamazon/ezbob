using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;
    using Ezbob.Backend.Models.ExternalAPI;

    public interface IEzBobApi {
        [OperationContract]
        AlibabaAvailableCreditActionResult CustomerAvailableCredit(string customerRefNum, long aliMemberID);

        [OperationContract]
        ActionMetaData RequalifyCustomer(string customerRefNum, long aliMemberID);

        [OperationContract]
        AlibabaSaleContractActionResult SaleContract(AlibabaContractDto dto);

        [OperationContract]
        ActionMetaData SaveApiCall(ApiCallData data);
    }
}
