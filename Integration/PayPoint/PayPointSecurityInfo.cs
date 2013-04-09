using EZBob.DatabaseLib.Common;

namespace PayPoint
{
    public class PayPointSecurityInfo : IMarketPlaceSecurityInfo
    {
        public PayPointSecurityInfo(int marketplaceId, string remotePassword, string vpnPassword, string mid)
        {
            MarketplaceId = marketplaceId;
            RemotePassword = remotePassword;
            VpnPassword = vpnPassword;
            Mid = mid;
        }

        public int MarketplaceId { get; set; }
        public string RemotePassword { get; set; }
        public string VpnPassword { get; set; }
        public string Mid { get; set; }
    }
}
