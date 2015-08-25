using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Strategies.Misc;
    using EzService.Interfaces;

    /// <summary>
    /// 'Land Registry' - registers the ownership of land and property in England and Wales
    /// </summary>
    internal class LandRegistry : Executor, IEzLandRegistry {
        /// <summary>
        /// Initializes a new instance of the <see cref="LandRegistry"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public LandRegistry(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Land Registry inquiry.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="buildingNumber">The building number.</param>
        /// <param name="buildingName">Name of the building.</param>
        /// <param name="streetName">Name of the street.</param>
        /// <param name="cityName">Name of the city.</param>
        /// <param name="postCode">The post code.</param>
        /// <returns></returns>
        public string LandRegistryInquiry(int userId, int customerId, string buildingNumber, string buildingName, string streetName, string cityName, string postCode) {
            LandRegistryEnquiry oInstance;
            ExecuteSync(out oInstance, customerId, userId, customerId, buildingNumber, buildingName, streetName, cityName, postCode);
            return oInstance.Result;
        }

        /// <summary>
        /// 'Land Registry' Register Extract Service (RES).
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="titleNumber">The title number.</param>
        /// <returns></returns>
        public string LandRegistryRes(int userId, int customerId, string titleNumber) {
            LandRegistryRes oInstance;
            ExecuteSync(out oInstance, customerId, userId, customerId, titleNumber);
            return oInstance.Result;
        }
    }
}
