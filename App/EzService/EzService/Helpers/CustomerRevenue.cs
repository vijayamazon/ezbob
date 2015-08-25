using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Strategies.CustomerManualAnnualizedRevenue;
    using EzService.Interfaces;

    /// <summary>
    /// Handles customer revenue
    /// </summary>
    internal class CustomerRevenue : Executor, IEzCustomerRevenue {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRevenue"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public CustomerRevenue(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Gets the customer manual annualized revenue.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        public CustomerManualAnnualizedRevenueActionResult GetCustomerManualAnnualizedRevenue(int customerID) {
            GetCustomerManualAnnualizedRevenue oInstance;

            ActionMetaData oResult = ExecuteSync(out oInstance, null, null, customerID);

            return new CustomerManualAnnualizedRevenueActionResult {
                MetaData = oResult,
                Value = oInstance.Result,
            };
        }

        /// <summary>
        /// Sets the customer manual annualized revenue.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="revenue">The revenue.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public CustomerManualAnnualizedRevenueActionResult SetCustomerManualAnnualizedRevenue(int customerID, decimal revenue, string comment) {
            SetCustomerManualAnnualizedRevenue oInstance;

            ActionMetaData oResult = ExecuteSync(out oInstance, null, null, customerID, revenue, comment);

            return new CustomerManualAnnualizedRevenueActionResult {
                MetaData = oResult,
                Value = oInstance.Result,
            };
        }
    }
}
