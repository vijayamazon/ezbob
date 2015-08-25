using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;
    using EchoSignLib;

    public interface IEzEchoSign {
        [OperationContract]
        ActionMetaData EsignProcessPending(int? customerID);

        [OperationContract]
        StringActionResult EsignSend(int userId, EchoSignEnvelope[] package);

        [OperationContract]
        EsignatureFileActionResult LoadEsignatureFile(int userId, long esignatureID);

        [OperationContract]
        EsignatureListActionResult LoadEsignatures(int userId, int? customerID, bool pollStatus);
    }
}
