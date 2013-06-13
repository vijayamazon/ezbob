namespace FreeAgent
{
	using System;
	using EZBob.DatabaseLib.Common;

	public class FreeAgentSecurityInfo : IMarketPlaceSecurityInfo
    {
		public FreeAgentSecurityInfo()
		{
			ValidUntil = DateTime.UtcNow.AddDays(-1);
		}

        public int MarketplaceId { get; set; }
		public string Name { get; set; }
		public string ApprovalToken { get; set; }
		public string AccessToken { get; set; }
		public string TokenType { get; set; }
		public int ExpiresIn { get; set; }
		public string RefreshToken { get; set; }
		public DateTime ValidUntil { get; set; }
    }
}
