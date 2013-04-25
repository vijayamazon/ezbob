using EZBob.DatabaseLib.Common;
namespace EKM
{
    public class EkmSecurityInfo : IMarketPlaceSecurityInfo
    {
        public int MarketplaceId { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }
}
