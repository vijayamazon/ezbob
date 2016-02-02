using System.Threading.Tasks;

namespace EzBob3dParties.SimplyPostcode {
    using EzBobModels.SimplyPostcode;

    internal interface ISimplyPostcodeService {
        /// <summary>
        /// Gets the addresses by post code.
        /// </summary>
        /// <param name="postCode">The post code.</param>
        /// <returns></returns>
        Task<SimplyPostCodeAddressSearchResponse> GetAddressesByPostCode(string postCode);

        /// <summary>
        /// Gets the address details.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<SimplyPostcodeDatailedAddress> GetAddressDetails(string id);
    }
}
