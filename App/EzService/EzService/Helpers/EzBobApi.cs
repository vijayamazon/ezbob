using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Models.ExternalAPI;
    using Ezbob.Backend.Strategies.ExternalAPI;
    using Ezbob.Backend.Strategies.ExternalAPI.Alibaba;
    using EzService.Interfaces;

    /// <summary>
    /// Represents company's API
    /// </summary>
    internal class EzBobApi : Executor, IEzBobApi {
        /// <summary>
        /// Initializes a new instance of the <see cref="EzBobApi"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public EzBobApi(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Customers the available credit.
        /// </summary>
        /// <param name="customerRefNum">The customer reference number.</param>
        /// <param name="aliMemberID">The ali member identifier.</param>
        /// <returns></returns>
        public AlibabaAvailableCreditActionResult CustomerAvailableCredit(string customerRefNum, long aliMemberID) {
            CustomerAvaliableCredit instance;

            Log.Info("ESI CustomerAvaliableCredit: customerID: {0}, customerID: {1}", customerRefNum, aliMemberID);

            ExecuteSync(out instance, null, null, customerRefNum, aliMemberID);

            return new AlibabaAvailableCreditActionResult {
                Result = instance.Result
            };

        }

        /// <summary>
        /// Re-qualifies the customer.
        /// </summary>
        /// <param name="customerRefNum">The customer reference number.</param>
        /// <param name="aliMemberID">The ali member identifier.</param>
        /// <returns></returns>
        public ActionMetaData RequalifyCustomer(string customerRefNum, long aliMemberID) {
            Log.Info("ESI RequalifyCustomer: customerID: {0}, customerID: {1}", customerRefNum, aliMemberID);

            ActionMetaData amd = Execute<RequalifyCustomer>(null, null, customerRefNum, aliMemberID);

            return amd;

        }

        /// <summary>
        /// Saves the API call.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public ActionMetaData SaveApiCall(ApiCallData data) {
            ActionMetaData amd = Execute<SaveApiCall>(data.CustomerID, null, data);

            return amd;

        }

        /// <summary>
        /// Sales the contract.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        public AlibabaSaleContractActionResult SaleContract(AlibabaContractDto dto) {
            SaleContract instance;

            ExecuteSync(out instance, null, null, dto);

            return new AlibabaSaleContractActionResult {
                Result = instance.Result
            };
        }
    }
}
