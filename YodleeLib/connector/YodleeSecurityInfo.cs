using EZBob.DatabaseLib.Common;
namespace YodleeLib.connector
{
    public class YodleeSecurityInfo : IMarketPlaceSecurityInfo
    {
        public int MarketplaceId { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }
}
