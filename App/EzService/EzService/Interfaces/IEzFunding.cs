using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;

    public interface IEzFunding {
        [OperationContract]
        AvailableFundsActionResult GetAvailableFunds(int underwriterId);

        [OperationContract]
        ActionMetaData RecordManualPacnetDeposit(int underwriterId, string underwriterName, int amount);

        [OperationContract]
        ActionMetaData DisableCurrentManualPacnetDeposits(int underwriterId);

        [OperationContract]
        ActionMetaData VerifyEnoughAvailableFunds(int underwriterId, decimal deductAmount);

        [OperationContract]
        ActionMetaData TopUpDelivery(int underwriterId, decimal amount, int contentCase);

        [OperationContract]
        ActionMetaData PacnetDelivery(int underwriterId, decimal amount);
    }
}
