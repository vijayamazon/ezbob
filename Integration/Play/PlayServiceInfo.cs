using System;
using EzBob.CommonLib;

namespace Integration.Play {
	public class PlayServiceInfo : IMarketplaceServiceInfo {
		public string DisplayName { get { return "Play"; } }
		public string Description { get { return "Play.com"; } }
		public Guid InternalId { get { return new Guid("{A5E96D38-FD2E-4E54-9E0C-276493C950A6}"); } }
	} // class PlayServiceInfo
} // namespace