using EZBob.DatabaseLib.Common;

namespace Integration.Volusion {
	public class VolusionSecurityInfo : IMarketPlaceSecurityInfo {
		public int MarketplaceId { get; set; }
		public string Url { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string DisplayName { get; set; }
	} // VolusionSecurityInfo
} // namespace
