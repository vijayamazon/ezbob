using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobDAL
{
    using EzBobCommon;
    using Models;

    public class DalService {
        private readonly CustomerQueries customerQueries = new CustomerQueries("Server=localhost;Database=ezbob;User Id=ezbobuser;Password=ezbobuser;MultipleActiveResultSets=true");

        [Injected]
        public CustomerQueries CustomerQueries { get; set; }

        public CustomerDetails GetCustomerDetails(int customerId) {
            return this.customerQueries.GetCustomerDetails(customerId);
        }
    }
}
