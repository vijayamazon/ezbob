using EZBob.DatabaseLib.Common;

namespace Integration.Play {
	public class PlaySecurityInfo : IMarketPlaceSecurityInfo {
		public int MarketplaceId { get; set; }
		public string Name { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
	} // PlaySecurityInfo
} // namespace
