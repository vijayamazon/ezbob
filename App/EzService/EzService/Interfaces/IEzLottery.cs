using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces
{
    using System.ServiceModel;
    using Ezbob.Backend.Models;

    public interface IEzLottery
    {
        [OperationContract]
        ActionMetaData ChangeLotteryPlayerStatus(int customerID, Guid playerID, LotteryPlayerStatus newStatus);

        [OperationContract]
        LotteryActionResult PlayLottery(int customerID, Guid playerID);

        [OperationContract]
        ActionMetaData EnlistLottery(int customerID);
    }
}
