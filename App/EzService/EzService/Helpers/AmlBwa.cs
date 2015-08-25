using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Strategies.Misc;
    using EzService.Interfaces;

    /// <summary>
    /// Anti-Money Laundering (AML)
    /// checks for person's address and bank account match) 
    /// 
    /// </summary>
    internal class AmlBwa : Executor, IEzAmlBwa {
        public AmlBwa(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Checks the aml.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public ActionMetaData CheckAml(int customerId, int userId) 
        {
            return Execute<AmlChecker>(customerId, userId, customerId);
        }

        /// <summary>
        /// Checks the aml.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="idhubHouseNumber">The house number.</param>
        /// <param name="idhubHouseName">Name of the idhub house.</param>
        /// <param name="idhubStreet">The street.</param>
        /// <param name="idhubDistrict">The district.</param>
        /// <param name="idhubTown">The town.</param>
        /// <param name="idhubCounty">The county.</param>
        /// <param name="idhubPostCode">The post code.</param>
        /// <returns></returns>
        public ActionMetaData CheckAmlCustom(int userId, int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet,
            string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode) 
        {
            return Execute<AmlChecker>(customerId, userId, customerId, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown, idhubCounty, idhubPostCode);
        }

        /// <summary>
        /// Checks the bwa.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public ActionMetaData CheckBwa(int customerId, int userId) 
        {
            return Execute<BwaChecker>(customerId, userId, customerId);
        }

        /// <summary>
        /// Checks the bwa.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="idhubHouseNumber">The house number.</param>
        /// <param name="idhubHouseName">Name of the house.</param>
        /// <param name="idhubStreet">The street.</param>
        /// <param name="idhubDistrict">The district.</param>
        /// <param name="idhubTown">The town.</param>
        /// <param name="idhubCounty">The county.</param>
        /// <param name="idhubPostCode">The post code.</param>
        /// <param name="idhubBranchCode">The branch code.</param>
        /// <param name="idhubAccountNumber">The account number.</param>
        /// <returns></returns>
        public ActionMetaData CheckBwaCustom(int userId, int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet,
            string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode,
            string idhubBranchCode, string idhubAccountNumber) 
        {
            return Execute<BwaChecker>(customerId, userId,
                customerId, idhubHouseNumber, idhubHouseName, idhubStreet,
                idhubDistrict, idhubTown, idhubCounty, idhubPostCode,
                idhubBranchCode, idhubAccountNumber
                );
        }
    }
}
