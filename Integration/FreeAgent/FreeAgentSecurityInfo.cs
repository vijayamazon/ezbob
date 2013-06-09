using EZBob.DatabaseLib.Common;
namespace FreeAgent
{
	public class FreeAgentSecurityInfo : IMarketPlaceSecurityInfo
    {
        public int MarketplaceId { get; set; }
		public string Name { get; set; }
		public string ApprovalToken { get; set; }
		public string AccessToken { get; set; }
		public string TokenType { get; set; }
		public int ExpiresIn { get; set; }
		public string RefreshToken { get; set; }
    }
}
