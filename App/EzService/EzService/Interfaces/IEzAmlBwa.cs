using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;

    public interface IEzAmlBwa {
        [OperationContract]
        ActionMetaData CheckAml(int customerId, int userId);

        [OperationContract]
        ActionMetaData CheckAmlCustom(
            int userId,
            int customerId,
            string idhubHouseNumber,
            string idhubHouseName,
            string idhubStreet,
            string idhubDistrict,
            string idhubTown,
            string idhubCounty,
            string idhubPostCode
            );

        [OperationContract]
        ActionMetaData CheckBwa(int customerId, int userId);

        [OperationContract]
        ActionMetaData CheckBwaCustom(
            int userId,
            int customerId,
            string idhubHouseNumber,
            string idhubHouseName,
            string idhubStreet,
            string idhubDistrict,
            string idhubTown,
            string idhubCounty,
            string idhubPostCode,
            string idhubBranchCode,
            string idhubAccountNumber
            );
    }
}
