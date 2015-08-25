using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Strategies.CreditSafe;
    using EzService.Interfaces;

    /// <summary>
    /// 'Credit Safe' - supplies company credit reports
    /// http://www2.creditsafeuk.com/
    /// </summary>
    internal class CreditSafe : Executor, IEzCreditSafe {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditSafe"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public CreditSafe(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Parses the credit safe LTD.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="serviceLogID">The service log identifier.</param>
        /// <returns></returns>
        public ActionMetaData ParseCreditSafeLtd(int customerID, int userID, long serviceLogID) {
            return Execute<ParseCreditSafeLtd>(customerID, userID, serviceLogID);
        }

        /// <summary>
        /// Parses the credit safe non LTD.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="serviceLogID">The service log identifier.</param>
        /// <returns></returns>
        public ActionMetaData ParseCreditSafeNonLtd(int customerID, int userID, long serviceLogID) {
            return Execute<ParseCreditSafeNonLtd>(customerID, userID, serviceLogID);
        }
    }
}
