using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;

    public interface IEzCustomerRevenue {
        [OperationContract]
        CustomerManualAnnualizedRevenueActionResult GetCustomerManualAnnualizedRevenue(int customerID);

        [OperationContract]
        CustomerManualAnnualizedRevenueActionResult SetCustomerManualAnnualizedRevenue(int customerID, decimal revenue, string comment);
    }
}
