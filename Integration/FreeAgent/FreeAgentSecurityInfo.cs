using EZBob.DatabaseLib.Common;
namespace FreeAgent
{
	public class FreeAgentSecurityInfo : IMarketPlaceSecurityInfo
    {
        public int MarketplaceId { get; set; }
		public string Name { get; set; }
		public string Token { get; set; }
    }
}
