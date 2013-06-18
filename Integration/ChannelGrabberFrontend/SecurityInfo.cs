using EZBob.DatabaseLib.Common;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	#region class SecurityInfo

	public class SecurityInfo : IMarketPlaceSecurityInfo {
		#region public

		public int MarketplaceId { get; set; }
		public AccountData AccountData { get; set; }

		#endregion public
	} // class SecurityInfo

	#endregion class SecurityInfo
} // namespace Integration.ChannelGrabberFrontend
