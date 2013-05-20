namespace YodleeLib.connector
{
	using EZBob.DatabaseLib.Common;

    public class YodleeSecurityInfo : IMarketPlaceSecurityInfo
    {
        public int MarketplaceId { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public long ItemId { get; set; }
    }
}
