namespace Sage
{
	using System;
	using EZBob.DatabaseLib.Common;

	public class SageSecurityInfo : IMarketPlaceSecurityInfo
    {
        public int MarketplaceId { get; set; }
		//public string Name { get; set; }
		public string ApprovalToken { get; set; }
		public string AccessToken { get; set; }
		public string TokenType { get; set; }
    }
}
