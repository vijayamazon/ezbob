using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;

    public interface IEzCreditSafe {
        [OperationContract]
        ActionMetaData ParseCreditSafeLtd(int customerID, int userID, long serviceLogID);

        [OperationContract]
        ActionMetaData ParseCreditSafeNonLtd(int customerID, int userID, long serviceLogID);
    }
}
