using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.HMRC
{
    using EzBobCommon;

    public interface IHmrcService {
        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<InfoAccumulator> ValidateCredentials(string userName, string password);
        /// <summary>
        /// Gets the vat returns.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<HmrcVatReturnsInfo> GetVatReturns(string userName, string password);
    }
}
