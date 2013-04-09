using EZBob.DatabaseLib.Common;

namespace Integration.Volusion {
    public class VolusionSecurityInfo : IMarketPlaceSecurityInfo {
        public int MarketplaceId { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    } // VolusionSecurityInfo
} // namespace
