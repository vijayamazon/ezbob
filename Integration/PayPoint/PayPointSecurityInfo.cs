using EZBob.DatabaseLib.Common;
namespace PayPoint
{
    public class PayPointSecurityInfo : IMarketPlaceSecurityInfo
    {
        public int MarketplaceId { get; set; }
        public string RemotePassword { get; set; }
        public string VpnPassword { get; set; }
        public string Mid { get; set; }
    }
}
