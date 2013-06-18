using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;
using Integration.ChannelGrabberConfig;
using StructureMap.Configuration.DSL;
using Integration.ChannelGrabberFrontend;

namespace Integration.Volusion {
	public class VolusionServiceInfo : ServiceInfo, IMarketplaceServiceInfo {
		public const string VendorName = "Volusion";

		public VolusionServiceInfo() {
			Load(VendorName);
		} // constructor
	} // VolusionServiceInfo

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			For<IMarketplaceType>().Use<DatabaseMarketPlace<VolusionServiceInfo>>().Named(VolusionServiceInfo.VendorName);
			For<DatabaseMarketplaceBase<FunctionType>>().Use<DatabaseMarketPlace<VolusionServiceInfo>>();
			For<IMarketplaceRetrieveDataHelper>().Use<RetriveDataHelper>().Named(VolusionServiceInfo.VendorName);
		} // constructor
	} // class PluginRegistry
} // namespace