namespace EzBob3dParties.SimplyPostcode {
    using System.Threading.Tasks;
    using EzBobCommon;
    using EzBobCommon.Web;
    using EzBobModels.SimplyPostcode;

    internal class SimplyPostcodeService : ISimplyPostcodeService{
        private static readonly string SearchForAddressTemplate = "http://www.simplylookupadmin.co.uk/JSONservice/JSONSearchForAddress.aspx?datakey={0}&postcode={1}";
        private static readonly string GetAddressRecordTemplate = "http://www.simplylookupadmin.co.uk/JSONservice/JSONGetAddressRecord.aspx?datakey={0}&id={1}";

        [Injected]
        public SimplyPostcodeConfig Config { get; set; }

        [Injected]
        public IEzBobHttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets the addresses by post code.
        /// </summary>
        /// <param name="postCode">The post code.</param>
        /// <returns></returns>
        public Task<SimplyPostCodeAddressSearchResponse> GetAddressesByPostCode(string postCode) {
            string url = string.Format(SearchForAddressTemplate, Config.DataKey, postCode);
            return HttpClient.GetAsyncJsonResponseAs<SimplyPostCodeAddressSearchResponse>(url);
        }

        /// <summary>
        /// Gets the address details.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Task<SimplyPostcodeDatailedAddress> GetAddressDetails(string id) {
            string url = string.Format(GetAddressRecordTemplate, Config.DataKey, id);
            return HttpClient.GetAsyncJsonResponseAs<SimplyPostcodeDatailedAddress>(url);
        }
    }
}
