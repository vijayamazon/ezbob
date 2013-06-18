using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;
using Integration.ChannelGrabberConfig;
using Integration.ChannelGrabberFrontend;
using StructureMap.Configuration.DSL;

namespace Integration.Play {
	public class PlayServiceInfo : ServiceInfo, IMarketplaceServiceInfo {
		public const string VendorName = "Play";

		public PlayServiceInfo() {
			Load(VendorName);
		} // constructor
	} // PlayServiceInfo

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			For<IMarketplaceType>().Use<DatabaseMarketPlace<PlayServiceInfo>>().Named(PlayServiceInfo.VendorName);
			For<DatabaseMarketplaceBase<FunctionType>>().Use<DatabaseMarketPlace<PlayServiceInfo>>();
			For<IMarketplaceRetrieveDataHelper>().Use<RetriveDataHelper>().Named(PlayServiceInfo.VendorName);
		} // constructor
	} // class PluginRegistry
} // namespace