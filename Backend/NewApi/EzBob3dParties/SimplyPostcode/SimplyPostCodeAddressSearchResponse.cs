using System.Collections.Generic;

namespace EzBob3dParties.SimplyPostcode
{
    using EzBobModels.SimplyPostcode;

    class SimplyPostCodeAddressSearchResponse
    {
        public int found { get; set; }
        public string credits_display_text { get; set; }
        public string accountadminpage { get; set; }
        public string errorMessage { get; set; }
        public int maxresults { get; set; }
        public int recordcount { get; set; }
        public IList<SimplyPostcodeAddress> Records { get; set; }
    }
}
