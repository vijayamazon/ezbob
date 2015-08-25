using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;

    public interface IEzLandRegistry {
        [OperationContract]
        string LandRegistryInquiry(int userId, int customerId, string buildingNumber, string buildingName,
            string streetName, string cityName, string postCode);

        [OperationContract]
        string LandRegistryRes(int userId, int customerId, string titleNumber);
    }
}
