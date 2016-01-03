using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Experian {
    using EzBobCommon;
    using EzBobModels.ThirdParties.Experian;

    public interface IExperian {
        /// <summary>
        /// Targets the business.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="postCode">The post code.</param>
        /// <param name="isLimited">if set to <c>true</c> [is limited].</param>
        /// <param name="regNum">The reg number.</param>
        /// <returns></returns>
        ResultInfoAccomulator<IEnumerable<Experian3dPartyCompanyInfo>> TargetBusiness(string companyName, string postCode, bool isLimited, string regNum = "");
    }
}
